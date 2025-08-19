using System;
using FinalParalela.Controllers;
using FinalParalela.Models;
using Microsoft.AspNetCore.Mvc;
using Renci.SshNet.Common;
using Xunit;


// [Fact] lo utilizamos para marcar cada metodo de la clase como un test unitario
public class TestableSshController : SshController
{
    // aqui sobreescribiremos los 2 metodos que manejan el analisis de log y la conexion ssh,sftp para el csv de los logs, esto ya que esos son los tests que vamos a evaluar
    public Action<SSHConnectionDto, string>? OnFetch { get; set; }
    public Func<string, object>? OnAnalyze { get; set; }

    // Sobrescribimos el hook que en prod hace SSH+SFTP; aquí lo simulamos
    protected override void GenerateAndDownloadCsv(SSHConnectionDto dto, string localPath)
    {
        OnFetch?.Invoke(dto, localPath);
    }

    // Sobrescribimos el metodo del analizador y devolvemos un resultado de analisis de logs simulado
    protected override object AnalyzeLogs(string localPath)
        => OnAnalyze != null ? OnAnalyze(localPath) : new { };
}

public class SshControllerSimpleTests
{
    // simulamos un dto valido, pero que no conectara a ningun servidor ni nada de red al ser un test unitario
    private static SSHConnectionDto DtoOk() => new SSHConnectionDto
    {
        Host = "localhost",
        Port = 22,
        User = "user",
        Password = "pass"
    };

    [Fact]
    public void CasoFeliz_DeberiaRetornarOkConResultado()
    {
        // Arrange: simulamos éxito en la “descarga” y retorno de análisis
        var controller = new TestableSshController
        {
            OnFetch = (dto, path) =>
            {
                // Si quieres, puedes incluso crear un archivo vacío:
                // System.IO.File.WriteAllText(path, "TimeCreated,LevelDisplayName,ProviderName,Id,Message\n");
                // Verificamos que la ruta termina como esperamos:
                Assert.EndsWith("system_log.csv", path.Replace('\\', '/'));
            },
            OnAnalyze = path => new { Total = 123, Paralelo = true } // Resultado simulado
        };

        // ejecutamos el endpoint con el dto que tenemos
        var res = controller.Run(DtoOk());

        //debemos de recibir un 200, que se traduce a OkObjectResult
        var ok = Assert.IsType<OkObjectResult>(res);

        // El controlador devuelve un objeto anonimo con un Mensaje y Resultado .
        // al ser anonimo, no podemos acceder a sus propiedades directamente, por lo que mapeamos su tipo

        var value = ok.Value!;
        var tipo = value.GetType();

        // Obtengo la propiedad mensaje y valido que exista y no sea nula
        var pMensaje = tipo.GetProperty("Mensaje");
        Assert.NotNull(pMensaje);
        var mensaje = pMensaje!.GetValue(value) as string;
        Assert.NotNull(mensaje);

        // verificamos que tenga el mensaje de analisis correcto, que devolvemos al hacer el analisis desde el endpoint
        Assert.Contains("Análisis paralelo", mensaje!);

        // hacemos exactament lo mismo con la propiedad del resultado, que es donde esta el resultado del analissi que se hizo
        var pResultado = tipo.GetProperty("Resultado");
        Assert.NotNull(pResultado);
        var resultado = pResultado!.GetValue(value);
        Assert.NotNull(resultado);

        // validamos que tenga los campos correctos
        var tRes = resultado!.GetType();
        Assert.Equal(123, (int)tRes.GetProperty("Total")!.GetValue(resultado)!);
        Assert.True((bool)tRes.GetProperty("Paralelo")!.GetValue(resultado)!);
    }

    [Fact]
    public void CredencialesInvalidas_DeberiaRetornarBadRequest()
    {
        // basicamente simulamos la excepcion de una autenticacion fallida
        var controller = new TestableSshController
        {
            OnFetch = (dto, path) => throw new SshAuthenticationException("Credenciales inválidas")
        };

        // ejecutamos el endpoint con el dto que tenemos
        var res = controller.Run(DtoOk());

        // debe de devolver un 400 por error de autenticacion
        var bad = Assert.IsType<BadRequestObjectResult>(res);
        Assert.Contains("Error de autenticación", bad.Value!.ToString());
    }

    [Fact]
    public void ConexionFalla_DeberiaRetornarBadRequest()
    {
        // Arrange: simulamos que el hook lanza SshConnectionException
        var controller = new TestableSshController
        {
            OnFetch = (dto, path) => throw new SshConnectionException("No se pudo conectar")
        };

        // ejecutamos el endpoint con el dto que tenemos
        var res = controller.Run(DtoOk());

        // error de conexion, deberia retornar un 400 con el mensaje adecuado
        var bad = Assert.IsType<BadRequestObjectResult>(res);
        Assert.Contains("Error de conexion SSH", bad.Value!.ToString());
    }

    [Fact]
    public void ErrorInesperado_DeberiaRetornarBadRequest()
    {
        // Arrange: simulamos cualquier excepcion que no estemos controlando
        var controller = new TestableSshController
        {
            OnFetch = (dto, path) => throw new Exception("Excepcion no controlada")
        };

        // ejecutamos el endpoint con el dto que tenemos
        var res = controller.Run(DtoOk());

        // debe de retornar 400 pore error inesperado
        var bad = Assert.IsType<BadRequestObjectResult>(res);
        Assert.Contains("Error inesperado", bad.Value!.ToString());
    }
}
