using KeenConveyance;
using WebAPISample.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddKeenConveyance()
                .ConfigureService(options =>
                {
                    options.ControllerSelectPredicate = typeInfo => typeInfo.IsAssignableTo(typeof(IApplicationService));
                    //options.UsePathHttpRequestEntryKeyGenerator();
                });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
#if NET9_0_OR_GREATER
builder.Services.AddOpenApi();
#endif

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
#if NET9_0_OR_GREATER
    app.MapOpenApi(); 
#endif
    app.MapSwaggerUI();
}

app.UseAuthorization();

app.UseKeenConveyance();

app.MapControllers();

app.Run();
