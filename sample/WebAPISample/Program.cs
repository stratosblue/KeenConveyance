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
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.UseKeenConveyance();

app.MapControllers();

app.Run();
