using UnityEngine;
using System.Collections;
//test
namespace Completed
{
	//Enemy hereda de MovingObject, nuestra clase base para objetos que pueden moverse. Player también hereda de esta
	public class Enemy : MovingObject
	{
		public int playerDamage; //Cantidad de puntos de vida que se restan al jugador al atacar
		public AudioClip attackSound1; //Clip de ataque al jugador 1
		public AudioClip attackSound2; //Clip de ataque al jugador 2


		private Animator animator; // Variable para almacenar una referencia al componente Animator del enemigo
		private Transform target; // Transform para intentar moverse hacia cada turno
		private bool skipMove; // Booleano para determinar si el enemigo debe saltar un turno o moverse en este turno


		// Start sobrescribe la función Start de la clase base
		protected override void Start()
		{
			//Registra este enemigo en nuestra instancia de GameManager agregándolo a una lista de objetos Enemy
			//Permite que el GameManager emita comandos de movimiento.
			GameManager.instance.AddEnemyToList(this);

			//Obtiene y almacena una referencia al componente Animator adjunto
			animator = GetComponent<Animator>();

			//Encuentra el GameObject "Player" usando su etiqueta y almacena una referencia a su componente transform
			target = GameObject.FindGameObjectWithTag("Player").transform;

			//Llama a la función Start de nuestra clase base MovingObject
			base.Start();
		}


		//Sobrescribe la función AttemptMove de MovingObject para que Enemy salte turnos, más detallada en MovingObject.cs
		protected override void AttemptMove<T>(int xDir, int yDir)
		{
			//Verifica si skipMove es true, lo hace falso y salta este turno
			if (skipMove)
			{
				skipMove = false;
				return;

			}

			//Llama a la función AttemptMove de MovingObject
			base.AttemptMove<T>(xDir, yDir);

			//Ya que Enemy se ha movido, establece skipMove en true para saltar el siguiente movimiento
			skipMove = true;
		}


		//MoveEnemy es llamado por el GameManager cada turno para indicar a cada Enemy que intente moverse hacia el jugador
		public void MoveEnemy()
		{
			//Declara variables para las direcciones de movimiento en los ejes X e Y, de -1 a 1
			//Nos permiten elegir entre las direcciones cardinales: arriba, abajo, izquierda y derecha
			int xDir = 0;
			int yDir = 0;

			//Si la diferencia en posiciones es aproximadamente cero (Epsilon):
			if (Mathf.Abs(target.position.x - transform.position.x) < float.Epsilon)

				/* Si la coordenada y de la posición del objetivo (jugador) es mayor que la coordenada y de esta posición del enemigo
				establece la dirección y en 1 (mover hacia arriba). Si no, establece en -1 (mover hacia abajo) */
				yDir = target.position.y > transform.position.y ? 1 : -1;

			//Si la diferencia en posiciones es mayor que Epsilon:
			else
				// Verifica si la posición x del objetivo es mayor que la posición x del enemigo y establece la dirección x en 1 (derecha), si no establece en -1 (izquierda)
				xDir = target.position.x > transform.position.x ? 1 : -1;

			// Llama a la función AttemptMove y pasa el parámetro genérico Player, porque Enemy se está moviendo y espera potencialmente encontrarse con un Player
			AttemptMove<Player>(xDir, yDir);
		}


		// Es llamado si Enemy intenta moverse a un espacio ocupado por un Player, sobrescribe la función OnCantMove de MovingObject
		protected override void OnCantMove<T>(T component)
		{
			if (!GameManager.instance.IsInCombat()) // Verificar si no estamos en combate
			{
				// Inicia combate en lugar de atacar directamente
				GameManager.instance.EnterCombat(this);
			}
		}
	}
}
