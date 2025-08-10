var builder = WebApplication.CreateBuilder(args);

// Agregamos los servicios necesarios en el builder
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //utilizamos swagger para tener una interfaz de consulta al API
    app.UseSwagger();
    //inicializamos la UI de swagger
    app.UseSwaggerUI();
}

//Para redireccion de http a https
app.UseHttpsRedirection();

app.UseAuthorization();

//Mapear todos los controladores registrados
app.MapControllers();

//Se corre la aplicacion
app.Run();
