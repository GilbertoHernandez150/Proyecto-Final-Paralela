var builder = WebApplication.CreateBuilder(args);

// Agregamos los servicios necesarios en el builder
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// Add services to the container.
builder.Services.AddCors(options =>
{
    // creamos una politica que permita cualquier origen
    options.AddPolicy(name: "AllowAnyOriginPolicy",
                      policy =>
                      {
                          policy.AllowAnyOrigin() //permitimos cualquier host
                                .AllowAnyHeader() //permitimos cualquier header
                                .AllowAnyMethod(); //permitimos cualquier metodo, a pesar de solo utilizar el POST
                      });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //utilizamos swagger para tener una interfaz de consulta al API
    app.UseSwagger();
    //inicializamos la UI de swagger
    app.UseSwaggerUI();
}

app.UseCors("AllowAnyOriginPolicy"); // utilizamos la politica del cors que generamos

//Para redireccion de http a https
app.UseHttpsRedirection();

app.UseAuthorization();

//Mapear todos los controladores registrados
app.MapControllers();

//Se corre la aplicacion
app.Run();
