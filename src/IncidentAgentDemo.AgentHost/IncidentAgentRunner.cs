using IncidentAgentDemo.Contracts;
using Microsoft.Extensions.Logging;
using OpenAI.Responses;

namespace IncidentAgentDemo.AgentHost;

public sealed class IncidentAgentRunner(
    OpenAiAgentClient agentClient,
    McpToolExecutor toolExecutor,
    ILogger<IncidentAgentRunner> logger)
{
    private const int MaxIterations = 10;

    public async Task<AgentResponse> RunAsync(string userPrompt, CancellationToken ct = default)
    {
        var trace = new List<AgentTraceStep>();

        try
        {
            AddTrace(trace, "Prompt", $"User prompt received: \"{Truncate(userPrompt, 120)}\"");

            var options = agentClient.BuildOptions();

            // Add the user message to the options input items
            options.InputItems.Add(ResponseItem.CreateUserMessageItem(userPrompt));

            for (var iteration = 0; iteration < MaxIterations; iteration++)
            {
                AddTrace(trace, "AgentLoop", $"Iteration {iteration + 1}: Sending to model...");

                var response = await agentClient.CreateResponseAsync(options, ct);

                // Collect function calls from the output
                var functionCalls = response.OutputItems.OfType<FunctionCallResponseItem>().ToList();

                if (functionCalls.Count == 0)
                {
                    // No tool calls — extract the final text response
                    AddTrace(trace, "Decision", "Agent decided: enough information gathered");
                    AddTrace(trace, "Complete", "Generating final response...");

                    var textContent = ExtractTextFromResponse(response);
                    AddTrace(trace, "Result", "Final response generated successfully");

                    return new AgentResponse
                    {
                        FinalAnswer = textContent,
                        TraceSteps = trace
                    };
                }

                // Process each function call
                AddTrace(trace, "Decision", $"Agent decided: tool call needed ({functionCalls.Count} tool(s))");

                foreach (var functionCall in functionCalls)
                {
                    var toolName = functionCall.FunctionName;
                    var toolArgs = functionCall.FunctionArguments?.ToString() ?? "{}";
                    var callId = functionCall.CallId;

                    AddTrace(trace, "ToolSelected", $"Tool selected: {toolName}");
                    AddTrace(trace, "ToolArgs", $"Arguments: {toolArgs}");

                    logger.LogInformation("[Agent] Tool call: {Tool}({Args})", toolName, toolArgs);

                    var toolResult = await toolExecutor.ExecuteAsync(toolName, toolArgs, ct);

                    AddTrace(trace, "ToolResponse", $"Tool '{toolName}' returned data ({toolResult.Length} chars)");

                    logger.LogInformation("[Agent] Tool result preview: {Preview}", Truncate(toolResult, 200));

                    // Add the function call and its output to the conversation for next iteration
                    options.InputItems.Add(functionCall);
                    options.InputItems.Add(ResponseItem.CreateFunctionCallOutputItem(callId, toolResult));
                }
            }

            AddTrace(trace, "Error", $"Agent loop exceeded {MaxIterations} iterations");
            return new AgentResponse
            {
                FinalAnswer = "I was unable to complete the request within the allowed number of steps.",
                TraceSteps = trace,
                IsError = true
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[Agent] Error during agent run");
            AddTrace(trace, "Error", $"Exception: {ex.Message}");

            return new AgentResponse
            {
                FinalAnswer = $"An error occurred: {ex.Message}",
                TraceSteps = trace,
                IsError = true
            };
        }
    }

    private static string ExtractTextFromResponse(ResponseResult response)
    {
        var parts = new List<string>();

        foreach (var item in response.OutputItems)
        {
            if (item is MessageResponseItem messageItem)
            {
                foreach (var part in messageItem.Content)
                {
                    if (!string.IsNullOrEmpty(part.Text))
                    {
                        parts.Add(part.Text);
                    }
                }
            }
        }

        return parts.Count > 0
            ? string.Join("\n", parts)
            : "No response text generated.";
    }

    private static void AddTrace(List<AgentTraceStep> trace, string stepType, string message)
    {
        trace.Add(new AgentTraceStep { StepType = stepType, Message = message });
    }

    private static string Truncate(string value, int maxLength)
        => value.Length <= maxLength ? value : value[..maxLength] + "...";
}
