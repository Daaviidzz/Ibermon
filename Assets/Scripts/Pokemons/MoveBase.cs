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

}
