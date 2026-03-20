namespace Assets.Scripts.Batalla
{
    // Clase estática para almacenar datos de la batalla y usarla de puente entre escenqa combate y jugador
    public static class BattleData
    {
        public static Pokemon WildPokemon;

        public static PokemonParty TrainerParty { get; set; }
        public static bool EsEntrenador { get; set; }
    }
}
