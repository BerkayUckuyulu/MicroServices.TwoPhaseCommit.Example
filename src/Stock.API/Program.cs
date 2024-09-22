var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/control", () =>
{
    return true;
});

app.MapGet("/commit", () =>
{
    return false;
});

app.MapGet("/rollback", () =>
{
    return true;
});

app.Run();

