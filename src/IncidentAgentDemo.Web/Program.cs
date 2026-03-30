using IncidentAgentDemo.AgentHost;
using IncidentAgentDemo.Web.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var apiBaseUrl = builder.Configuration["Api:BaseUrl"] ?? "http://localhost:5006";

builder.Services.AddIncidentAgent(apiBaseUrl, options =>
{
    var section = builder.Configuration.GetSection("PromptAssets");
    section.Bind(options);

    // Resolve relative paths against the content root
    if (!Path.IsPathRooted(options.AgentsFilePath))
        options.AgentsFilePath = Path.GetFullPath(options.AgentsFilePath, builder.Environment.ContentRootPath);
    if (!Path.IsPathRooted(options.SkillsRootPath))
        options.SkillsRootPath = Path.GetFullPath(options.SkillsRootPath, builder.Environment.ContentRootPath);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
