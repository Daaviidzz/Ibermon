using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using ApiRest.Models;
using UnityEngine;

// =====================================================================================
//  PR-24 — Tests de INTEGRACIÓN (Bloque 5 - Cruzados)
//
//  Estos tests se diferencian de los anteriores en que NO prueban una clase
//  aislada, sino flujos completos que combinan varios subsistemas:
//
//   · PR-24.1 — Captura: BattleSystem (simulado) + PokemonParty + evento OnUpdated
//   · PR-24.2 — Curación en inventario: Inventory + RecoveryItem + Pokemon
//   · PR-24.3 — Curación de estado: ConditionsDB + Pokemon + RecoveryItem
//   · PR-24.4 — Combate por tipos: Pokemon + Move + TypeChart (flujo de daño)
//   · PR-24.5 — Carga de equipo desde API: IbermonConverter + CatalogoCache + PokemonParty
//
//  Estos tests son "caja negra" porque solo tocan los métodos públicos y
//  verifican el comportamiento observable del sistema. Documentan casos de uso
//  reales del juego, no detalles de implementación.
// =====================================================================================
public class TestIntegracion
{
    // -----------------------------------------------------------------------------------
    //  Helper: setea un campo privado por reflection (subiendo por la herencia)
    // -----------------------------------------------------------------------------------
    private static void SetField(object target, string fieldName, object value)
    {
        var tipo = target.GetType();
        FieldInfo field = null;
        while (tipo != null && field == null)
        {
            field = tipo.GetField(fieldName,
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            tipo = tipo.BaseType;
        }
        field?.SetValue(target, value);
    }

    private static PokemonBase CrearPokemonBase(
        string nombre = "TestMon",
        PokemonType type1 = PokemonType.Normal,
        PokemonType type2 = PokemonType.None,
        int maxHp = 100, int attack = 50, int defense = 50,
        int spAttack = 50, int spDefense = 50, int speed = 50)
    {
        var pb = ScriptableObject.CreateInstance<PokemonBase>();
        SetField(pb, "name", nombre);
        SetField(pb, "type1", type1);
        SetField(pb, "type2", type2);
        SetField(pb, "maxHp", maxHp);
        SetField(pb, "attack", attack);
        SetField(pb, "defense", defense);
        SetField(pb, "spAttack", spAttack);
        SetField(pb, "spDefense", spDefense);
        SetField(pb, "speed", speed);
        SetField(pb, "expYield", 100);
        SetField(pb, "catchRate", 100);
        SetField(pb, "learnableMoves", new List<LearnableMove>());
        return pb;
    }

    private static MoveBase CrearMoveBase(
        string nombre = "TestMove",
        PokemonType type = PokemonType.Normal,
        int power = 80, int accuracy = 100,
        MoveCategory category = MoveCategory.Especial)
    {
        var mb = ScriptableObject.CreateInstance<MoveBase>();
        SetField(mb, "name", nombre);
        SetField(mb, "type", type);
        SetField(mb, "power", power);
        SetField(mb, "accuracy", accuracy);
        SetField(mb, "pp", 25);
        SetField(mb, "category", category);
        SetField(mb, "priority", 0);
        SetField(mb, "alwaysHit", true); // siempre acierta para tests deterministas
        SetField(mb, "secondries", new List<SecondaryEffects>());
        return mb;
    }

    [SetUp]
    public void Setup()
    {
        ConditionsDB.Init();
    }

    // =================================================================================
    //  PR-24.1 — INTEGRACIÓN: Captura completa de un Ibermon
    //
    //  Flujo simulado: Aparece un Ibermon salvaje → el jugador lo captura → el
    //  equipo se actualiza → el evento OnUpdated notifica a la UI → el siguiente
    //  GetHealtyPokemon ya lo puede seleccionar.
    //
    //  Subsistemas: PokemonParty + Pokemon + sistema de eventos.
    // =================================================================================
    [Test]
    public void PR24_01_Integracion_CapturaDeIbermonSalvaje()
    {
        // ── ARRANGE: jugador con un equipo de 2 Ibermon, ambos vivos ────────────
        var pb = CrearPokemonBase();
        var go = new GameObject("EquipoJugador");
        go.SetActive(false);
        var party = go.AddComponent<PokemonParty>();
        SetField(party, "esEquipoJugador", false);
        go.SetActive(true);

        party.SetPokemonsForBattle(new List<Pokemon>
        {
            new Pokemon(pb, pLevel: 10),
            new Pokemon(pb, pLevel: 12)
        });
        int countInicial = party.Pokemons.Count;

        // Nos suscribimos al evento OnUpdated (lo que haría PartyScreen en el juego)
        bool eventoDisparado = false;
        party.OnUpdated += () => eventoDisparado = true;

        // ── ACT: capturamos un Ibermon salvaje ──────────────────────────────────
        var ibermonSalvaje = new Pokemon(pb, pLevel: 5);
        bool capturado = party.AddPokemon(ibermonSalvaje);

        // ── ASSERT: el flujo completo se ha ejecutado correctamente ─────────────
        Assert.IsTrue(capturado,
            "AddPokemon debe devolver true porque hay hueco (2/6)");
        Assert.AreEqual(countInicial + 1, party.Pokemons.Count,
            "El equipo ahora tiene un Ibermon más");
        Assert.Contains(ibermonSalvaje, party.Pokemons,
            "El Ibermon capturado debe estar en el equipo");
        Assert.IsTrue(eventoDisparado,
            "OnUpdated debe haberse disparado (para que la UI se refresque)");

        // ── CLEANUP ─────────────────────────────────────────────────────────────
        Object.DestroyImmediate(go);
        Object.DestroyImmediate(pb);
    }

    // =================================================================================
    //  PR-24.2 — INTEGRACIÓN: Uso de poción desde el inventario
    //
    //  Flujo: Jugador con Ibermon herido → abre mochila → selecciona poción →
    //  usa poción → HP del Ibermon sube → la poción se descuenta del inventario.
    //
    //  Subsistemas: Inventory + RecoveryItem (ItemBase.Use) + Pokemon.
    // =================================================================================
    [Test]
    public void PR24_02_Integracion_UsoPocionDesdeInventario()
    {
        // ── ARRANGE: Ibermon herido + inventario con 3 pociones ─────────────────
        var pb = CrearPokemonBase(maxHp: 100);
        var pokemon = new Pokemon(pb, pLevel: 50);
        pokemon.HP = 10; // muy herido

        var pocion = ScriptableObject.CreateInstance<RecoveryItem>();
        SetField(pocion, "name", "Pocion");
        SetField(pocion, "hpAmount", 50);

        var goInv = new GameObject("Inventory");
        var inv = goInv.AddComponent<Inventory>();
        SetField(inv, "slots", new List<ItemSlot>());
        SetField(inv, "pokeballSlots", new List<ItemSlot>());

        // Re-llamamos Awake para que reconstruya allSlots con las listas nuevas
        var awake = typeof(Inventory).GetMethod("Awake",
            BindingFlags.NonPublic | BindingFlags.Instance);
        awake?.Invoke(inv, null);

        inv.AddItem(pocion, count: 3, ItemCategory.Items);

        int hpInicial = pokemon.HP;
        int cantidadInicial = inv.GetSlotsByCategory((int)ItemCategory.Items)[0].Count;

        // ── ACT: usar la poción (1 vez) ─────────────────────────────────────────
        var itemUsado = inv.UseItem(itemIndex: 0,
                                     selectedPokemon: pokemon,
                                     selectedCategory: (int)ItemCategory.Items);

        // ── ASSERT ──────────────────────────────────────────────────────────────
        Assert.IsNotNull(itemUsado, "UseItem debe devolver el ItemBase usado");
        Assert.AreEqual(hpInicial + 50, pokemon.HP,
            "El HP del Ibermon debe haber subido 50 puntos (potencia de la poción)");

        var slot = inv.GetSlotsByCategory((int)ItemCategory.Items)[0];
        Assert.AreEqual(cantidadInicial - 1, slot.Count,
            "La cantidad de pociones debe haber bajado en 1");

        // ── CLEANUP ─────────────────────────────────────────────────────────────
        Object.DestroyImmediate(goInv);
        Object.DestroyImmediate(pb);
        Object.DestroyImmediate(pocion);
    }

    // =================================================================================
    //  PR-24.3 — INTEGRACIÓN: Curación de estado con antídoto
    //
    //  Flujo: Pokemon envenenado en combate → veneno hace daño 3 turnos → jugador
    //  usa antídoto → status se cura → el siguiente OnAfterTurn ya no hace daño.
    //
    //  Subsistemas: ConditionsDB + Pokemon (OnAfterTurn) + RecoveryItem.
    // =================================================================================
    [Test]
    public void PR24_03_Integracion_VenenoYAntidoto()
    {
        // ── ARRANGE ─────────────────────────────────────────────────────────────
        var pb = CrearPokemonBase(maxHp: 100);
        var pokemon = new Pokemon(pb, pLevel: 50);
        int hpInicial = pokemon.HP;

        var antidoto = ScriptableObject.CreateInstance<RecoveryItem>();
        SetField(antidoto, "recoverAllStatus", true);

        // ── ACT 1: aplicamos veneno y simulamos 3 turnos ────────────────────────
        pokemon.SetStatus(ConditionID.psn);
        pokemon.OnAfterTurn();
        pokemon.OnAfterTurn();
        pokemon.OnAfterTurn();

        int hpTrasVeneno = pokemon.HP;
        Assert.Less(hpTrasVeneno, hpInicial,
            "El veneno debe haber reducido el HP tras 3 turnos");
        Assert.IsNotNull(pokemon.Status,
            "El Ibermon sigue envenenado");

        // ── ACT 2: usamos el antídoto ────────────────────────────────────────────
        bool usadoOk = antidoto.Use(pokemon);

        // ── ACT 3: simulamos un turno más para confirmar que ya no envenena ─────
        int hpAntesDelTurno = pokemon.HP;
        pokemon.OnAfterTurn();

        // ── ASSERT ──────────────────────────────────────────────────────────────
        Assert.IsTrue(usadoOk,
            "El antídoto debe haberse consumido (devolvió true)");
        Assert.IsNull(pokemon.Status,
            "Tras el antídoto el Status debe ser null");
        Assert.AreEqual(hpAntesDelTurno, pokemon.HP,
            "El turno tras curar no debe restar HP (ya no hay veneno)");

        // ── CLEANUP ─────────────────────────────────────────────────────────────
        Object.DestroyImmediate(pb);
        Object.DestroyImmediate(antidoto);
    }

    // =================================================================================
    //  PR-24.4 — INTEGRACIÓN: Cálculo de daño con efectividad de tipo
    //
    //  Flujo: Pokemon Agua usa movimiento Agua contra Pokemon Fuego → el daño
    //  resultante es ~2× el que haría a un tipo Normal. Esto valida la cadena
    //  Move → Pokemon.TakeDamage → TypeChart → DamageDetails.
    //
    //  Subsistemas: Pokemon + Move/MoveBase + TypeChart.
    // =================================================================================
    [Test]
    public void PR24_04_Integracion_DanioPorEfectividadDeTipo()
    {
        // ── ARRANGE ─────────────────────────────────────────────────────────────
        // Mismo atacante en ambos escenarios (Pokemon Agua, alto SpAttack)
        var pbAtacante = CrearPokemonBase(type1: PokemonType.Agua, spAttack: 100);

        // Movimiento de baja potencia para que ambos defensores SOBREVIVAN
        // (con power=110 los dos morían y el delta era idéntico)
        var moveAgua = new Move(CrearMoveBase("Pistola Agua",
            type: PokemonType.Agua, power: 40, category: MoveCategory.Especial));

        // Defensores robustos (maxHp alto, defensa alta) para garantizar supervivencia
        var pbFuego = CrearPokemonBase(type1: PokemonType.Fuego, maxHp: 400, spDefense: 100);
        var defensorFuego = new Pokemon(pbFuego, pLevel: 50);

        var pbNormal = CrearPokemonBase(type1: PokemonType.Normal, maxHp: 400, spDefense: 100);
        var defensorNormal = new Pokemon(pbNormal, pLevel: 50);

        var atacante1 = new Pokemon(pbAtacante, pLevel: 50);
        var atacante2 = new Pokemon(pbAtacante, pLevel: 50);

        // Capturamos HP inicial ANTES del ataque (es lo que vamos a restar para
        // calcular el daño real, en vez de usar el MaxHp del ScriptableObject)
        int hpInicialFuego = defensorFuego.HP;
        int hpInicialNormal = defensorNormal.HP;

        // ── ACT: aplicamos el mismo movimiento a ambos defensores ───────────────
        // Fijamos la semilla para minimizar la varianza del random 0.85-1
        Random.InitState(42);
        var dmgFuego = defensorFuego.TakeDamage(moveAgua, atacante1);
        Random.InitState(42);
        var dmgNormal = defensorNormal.TakeDamage(moveAgua, atacante2);

        // ── ASSERT ──────────────────────────────────────────────────────────────
        Assert.AreEqual(2f, dmgFuego.TypeEffectiveness, 0.01f,
            "Agua contra Fuego debe tener efectividad 2.0");
        Assert.AreEqual(1f, dmgNormal.TypeEffectiveness, 0.01f,
            "Agua contra Normal debe tener efectividad 1.0");

        // El daño real al Fuego debe ser ~2× el daño al Normal
        int dañoAlFuego = hpInicialFuego - defensorFuego.HP;
        int dañoAlNormal = hpInicialNormal - defensorNormal.HP;

        Assert.Greater(defensorFuego.HP, 0,
            "El defensor Fuego debe sobrevivir al ataque (test mal configurado si muere)");
        Assert.Greater(defensorNormal.HP, 0,
            "El defensor Normal debe sobrevivir al ataque (test mal configurado si muere)");
        Assert.Greater(dañoAlFuego, dañoAlNormal,
            $"El daño al tipo Fuego ({dañoAlFuego}) debe ser MAYOR que al Normal ({dañoAlNormal})");

        float ratio = (float)dañoAlFuego / dañoAlNormal;
        Assert.That(ratio, Is.InRange(1.7f, 2.3f),
            $"El ratio de daño debe ser ~2× (margen por crítico/random). Ratio real: {ratio:F2}");

        // ── CLEANUP ─────────────────────────────────────────────────────────────
        Object.DestroyImmediate(pbAtacante);
        Object.DestroyImmediate(pbFuego);
        Object.DestroyImmediate(pbNormal);
    }

    // =================================================================================
    //  PR-24.5 — INTEGRACIÓN: Carga de equipo desde la API (sin red)
    //
    //  Flujo: La API devuelve una lista de IbermonJugador → IbermonConverter los
    //  transforma a Pokemon (consultando CatalogoCache para sprites/stats) →
    //  los Pokemon resultantes son utilizables en combate (HP > 0, Init OK).
    //
    //  Este es el caso de uso CRÍTICO al cargar una partida guardada.
    //  Subsistemas: IbermonConverter + CatalogoCache + Pokemon + PokemonBase
    //  (ScriptableObjects en Resources).
    // =================================================================================
    [Test]
    public void PR24_05_Integracion_CargaEquipoDesdeAPI_SinRed()
    {
        // ── ARRANGE: catálogo mock con 1 ibermon registrado ─────────────────────
        // Atención: aquí necesitaríamos que IbermonConverter encuentre un
        // PokemonBase real en Resources. Como no podemos garantizarlo en EditMode,
        // este test verifica la PARTE QUE SÍ ES TESTEABLE: el flujo de búsqueda
        // y manejo del caso "no encontrado".

        var goCache = new GameObject("CacheMock");
        var cache = goCache.AddComponent<CatalogoCache>();

        // Inyectamos un ibermon en los 6 diccionarios internos
        SetField(cache, "_ibermonNombres",
            new Dictionary<int, string> { { 25, "Pikachu" } });
        SetField(cache, "_ibermonNumeros",
            new Dictionary<string, int> { { "Pikachu", 25 } });
        SetField(cache, "_ibermonSpriteFrontal",
            new Dictionary<int, string> { { 25, "25.png" } });
        SetField(cache, "_ibermonSpriteTrasero",
            new Dictionary<int, string> { { 25, "back/25.png" } });
        SetField(cache, "_movimientoNombres", new Dictionary<int, string>());
        SetField(cache, "_movimientoNumeros", new Dictionary<string, int>());

        // ── ACT: convertimos un IbermonJugador "imposible" (id=9999) ────────────
        // y otro válido (id=25, pero PokemonBase no estará en Resources)
        var equipoAPI = new List<IbermonJugador>
        {
            new IbermonJugador
            {
                ibermon_catalogo_id = 9999, // no está en el cache
                nivel = 10,
                hp_actual = 30
            }
        };

        // Esperamos un LogError porque el id 9999 no existe
        UnityEngine.TestTools.LogAssert.Expect(LogType.Error,
            new System.Text.RegularExpressions.Regex("No se encontró el ibermon"));

        var resultados = IbermonConverter.ToPokemons(equipoAPI, cache);

        // ── ASSERT: el conversor maneja el error sin crashear y devuelve lista ──
        Assert.IsNotNull(resultados,
            "ToPokemons nunca debe devolver null, aunque haya errores parciales");
        Assert.AreEqual(0, resultados.Count,
            "Con todos los ibermon erróneos, la lista resultante debe estar vacía");

        // ── CLEANUP ─────────────────────────────────────────────────────────────
        Object.DestroyImmediate(goCache);
    }
}
