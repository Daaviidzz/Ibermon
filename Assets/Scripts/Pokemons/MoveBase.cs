using UnityEngine;
//Para crear un nuevo asset de tipo MoveBase desde el menu de Unity
[CreateAssetMenu(fileName = "Move", menuName = "Pokemons/New Move")]
public class MoveBase : ScriptableObject
{
    [SerializeField] string name;
    [TextArea]
    [SerializeField] string description;

    [SerializeField] PokemonType type;
    [SerializeField] int power;
    //precision del movimiento
    [SerializeField] int accuracy;
    //numero de veces que se puede usar el movimiento
    [SerializeField] int pp;

    //Property getters
    public string Name { get { return name; } }
    public string Description { get { return description; } }
    public PokemonType Type { get { return type; } }
    public int Power { get { return power; } }
    public int Accuracy { get { return accuracy; } }
    public int Pp { get { return pp; } }

    public bool IsSpecial
    {
        get
        {
            //Ejemplo simple: movimientos de tipo Fuego, Agua, Planta son especiales
            if(type == PokemonType.Fuego || type == PokemonType.Agua || type == PokemonType.Planta
                || type == PokemonType.Electrico || type == PokemonType.Hielo || type == PokemonType.Psiquico || type == PokemonType.Dragon
                || type==PokemonType.Lucha || type==PokemonType.Roca|| type==PokemonType.Tierra || type == PokemonType.Volador || type == PokemonType.Bicho || type == PokemonType.Fantasma || 
                type==PokemonType.Veneno)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}
