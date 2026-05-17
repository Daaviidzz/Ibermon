using NUnit.Framework;
using ApiRest.Models;
using UnityEngine;

// =====================================================================================
//  PR-05 — Tests para modelos DTOs (Bloque 1 - API REST)  [v2 corregido]
//
//  Los DTOs (Data Transfer Objects) son POCO con [System.Serializable] que sirven
//  para serializar/deserializar JSON entre Unity y la API.
//
//  Nota importante: JsonUtility de Unity NO soporta tipos nullable (int?, etc.) de
//  forma natural — los serializa como objetos con campos {value, hasValue}. Por eso
//  el DTO IbermonJugadorActualizarRequest (que tiene int? nivel, etc.) se debe
//  validar comprobando los nombres de campo en el JSON, no los valores literales.
// =====================================================================================
public class TestModelosDTO
{
    // -----------------------------------------------------------------------------------
    //  PR-05.1 - TokenResponse: serializar/deserializar conserva el access_token
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR05_01_TokenResponse_RoundTrip()
    {
        var original = new TokenResponse
        {
            access_token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
            token_type = "bearer"
        };

        string json = JsonUtility.ToJson(original);
        var deserializado = JsonUtility.FromJson<TokenResponse>(json);

        Assert.AreEqual(original.access_token, deserializado.access_token);
        Assert.AreEqual(original.token_type, deserializado.token_type);

        // El JSON producido debe contener los nombres en snake_case (estilo FastAPI)
        Assert.IsTrue(json.Contains("\"access_token\""),
            "El campo debe llamarse \"access_token\" (snake_case) en el JSON");
        Assert.IsTrue(json.Contains("\"token_type\""),
            "El campo debe llamarse \"token_type\" (snake_case) en el JSON");
    }

    // -----------------------------------------------------------------------------------
    //  PR-05.2 - PartidaCompleta: serializa correctamente con todos los campos
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR05_02_PartidaCompleta_TodosLosCampos()
    {
        var p = new PartidaCompleta
        {
            id = "abc123",
            nombre = "Test",
            personaje_elegido = "torrente",
            mapa_actual = "PuebloFuenlabrada",
            dinero = 500,
            tiempo_jugado = 3600,
            combates_ganados = 5,
            combates_perdidos = 2,
            starter_elegido = 1
        };

        string json = JsonUtility.ToJson(p);
        var rec = JsonUtility.FromJson<PartidaCompleta>(json);

        Assert.AreEqual(p.id, rec.id);
        Assert.AreEqual(p.nombre, rec.nombre);
        Assert.AreEqual(p.personaje_elegido, rec.personaje_elegido);
        Assert.AreEqual(p.mapa_actual, rec.mapa_actual);
        Assert.AreEqual(p.dinero, rec.dinero);
        Assert.AreEqual(p.tiempo_jugado, rec.tiempo_jugado);
        Assert.AreEqual(p.combates_ganados, rec.combates_ganados);
        Assert.AreEqual(p.starter_elegido, rec.starter_elegido);
    }

    // -----------------------------------------------------------------------------------
    //  PR-05.3 - IbermonJugador con lista de movimientos
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR05_03_IbermonJugador_ListaMovimientos()
    {
        var ib = new IbermonJugador
        {
            id = "ibj_001",
            ibermon_catalogo_id = 25,
            nivel = 15,
            hp_actual = 40,
            experiencia = 3500,
            movimientos_aprendidos = new System.Collections.Generic.List<MovimientoAprendido>
            {
                new MovimientoAprendido { numero = 85, pp = 10 },
                new MovimientoAprendido { numero = 33, pp = 25 }
            }
        };

        string json = JsonUtility.ToJson(ib);
        var rec = JsonUtility.FromJson<IbermonJugador>(json);

        Assert.AreEqual(25, rec.ibermon_catalogo_id);
        Assert.AreEqual(15, rec.nivel);
        Assert.AreEqual(2, rec.movimientos_aprendidos.Count);
        Assert.AreEqual(85, rec.movimientos_aprendidos[0].numero);
        Assert.AreEqual(10, rec.movimientos_aprendidos[0].pp);
    }

    // -----------------------------------------------------------------------------------
    //  PR-05.4 - IbermonJugadorActualizarRequest tiene los campos esperados.
    //  (No usamos JsonUtility porque omite tipos Nullable<T>. Verificamos la
    //   estructura del DTO directamente vía reflection: existencia y tipo de campo.)
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR05_04_ActualizarRequest_ContieneCamposEsperados()
    {
        var tipo = typeof(IbermonJugadorActualizarRequest);

        var nivel = tipo.GetField("nivel");
        var experiencia = tipo.GetField("experiencia");
        var hpActual = tipo.GetField("hp_actual");
        var movimientos = tipo.GetField("movimientos_aprendidos");
        var nickname = tipo.GetField("nickname");

        Assert.IsNotNull(nivel, "El DTO debe tener un campo 'nivel'");
        Assert.IsNotNull(experiencia, "El DTO debe tener un campo 'experiencia'");
        Assert.IsNotNull(hpActual, "El DTO debe tener un campo 'hp_actual'");
        Assert.IsNotNull(movimientos, "El DTO debe tener un campo 'movimientos_aprendidos'");
        Assert.IsNotNull(nickname, "El DTO debe tener un campo 'nickname'");

        Assert.AreEqual(typeof(int?), nivel.FieldType, "nivel debe ser int? (nullable)");
        Assert.AreEqual(typeof(int?), experiencia.FieldType, "experiencia debe ser int? (nullable)");
        Assert.AreEqual(typeof(int?), hpActual.FieldType, "hp_actual debe ser int? (nullable)");
        Assert.AreEqual(typeof(string), nickname.FieldType, "nickname debe ser string");

        Assert.IsTrue(tipo.IsDefined(typeof(System.SerializableAttribute), false),
            "El DTO debe estar marcado con [Serializable]");
    }

    // -----------------------------------------------------------------------------------
    //  PR-05.5 - UsuarioRegistroRequest contiene email, username y password
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR05_05_UsuarioRegistroRequest_CamposObligatorios()
    {
        var req = new UsuarioRegistroRequest
        {
            email = "test@test.com",
            username = "testuser",
            password = "passSegura123"
        };

        string json = JsonUtility.ToJson(req);

        Assert.IsTrue(json.Contains("\"email\":\"test@test.com\""));
        Assert.IsTrue(json.Contains("\"username\":\"testuser\""));
        Assert.IsTrue(json.Contains("\"password\":\"passSegura123\""));
    }
}
