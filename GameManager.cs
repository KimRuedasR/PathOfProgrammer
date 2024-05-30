using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace Completed
{
	using System.Collections.Generic; //Uso de listas 
	using UnityEngine.UI; //Usar UI.

	public class GameManager : MonoBehaviour
	{
		public float levelStartDelay = 2f; //Tiempo de espera antes de comenzar el nivel, en segundos
		public float turnDelay = 0.1f; //Retraso entre cada turno
		public int playerFoodPoints = 3; //Valor inicial de los puntos de comida del jugador
		public static GameManager instance = null; //GameManager accesible desde cualquier otro script
		[HideInInspector] public bool playersTurn = true; //Booleano para verificar si es el turno del jugador
		[HideInInspector] public bool isInCombat = false; // Booleano para verificar si el juego está en combate

		private Text levelText; //Texto para mostrar el número de nivel actual
		private GameObject levelImage; //Imagen para bloquear el nivel mientras se configura
		private BoardManager boardScript; //Almacena una referencia a BoardManager que configura el nivel
		private int level = 1; //Número de nivel actual
		private List<Enemy> enemies; //Lista de todas las unidades Enemy para emitirles comandos de movimiento
		private bool enemiesMoving; //Booleano para verificar si los enemigos se están moviendo
		private bool doingSetup = true; //Booleano para verificar configuracion del tablero, previene que el jugador se mueva durante la configuración

		//Awake siempre se llama antes de cualquier función Start
		void Awake()
		{
			//Verifica si la instancia ya existe.
			if (instance == null)
				instance = this;
			else if (instance != this)
				//Esto aplica nuestro patrón singleton, lo que significa que solo puede haber una instancia de GameManager
				Destroy(gameObject);

			//No se destruye al recargar la escena
			DontDestroyOnLoad(gameObject);

			//Asigna enemigos a una nueva lista de objetos
			enemies = new List<Enemy>();

			//Obtén una referencia al script BoardManager adjunto
			boardScript = GetComponent<BoardManager>();

			//Llama a la función InitGame para iniciar el primer nivel
			InitGame();
		}

		//Se llama solo una vez y el parámetro indica que se llame solo después de que se haya cargado la escena
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		static public void CallbackInitialization()
		{
			//Registra el callback para que se llame cada vez que se carga la escena
			SceneManager.sceneLoaded += OnSceneLoaded;
		}

		//Cada vez que se carga una escena
		static private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
		{
			instance.level++;
			instance.InitGame();
		}

		//Inicializa el juego para cada nivel.
		void InitGame()
		{
			//el jugador no puede moverse, previene que se mueva mientras la tarjeta de título está activa
			doingSetup = true;

			//Referencia a nuestra imagen LevelImage encontrándola por nombre 
			levelImage = GameObject.Find("LevelImage");

			//Referencia a nuestro texto LevelText encontrándolo por nombre y llamando a GetComponent
			levelText = GameObject.Find("LevelText").GetComponent<Text>();

			//Establece el texto de levelText al string "Day" y añade el número de nivel actual
			levelText.text = "Day " + level;

			//Activa levelImage bloqueando la vista del jugador del tablero de juego durante la configuración
			levelImage.SetActive(true);

			//Llama a la función HideLevelImage con un retraso en segundos de levelStartDelay
			Invoke("HideLevelImage", levelStartDelay);

			//Limpia cualquier objeto Enemy en nuestra lista para prepararse para el siguiente nivel
			enemies.Clear();

			//Llama a la función SetupScene del script BoardManager, pasa el número de nivel actual
			boardScript.SetupScene(level);
		}

		//Desactiva la imagen negra utilizada entre niveles
		void HideLevelImage()
		{
			//Desactiva la imagen de nivel
			levelImage.SetActive(false);

			//Establece doingSetup en falso permitiendo que el jugador se mueva nuevamente
			doingSetup = false;
		}

		//Update se llama una vez por frame
		void Update()
		{
			//Revisa que playersTurn, enemiesMoving o doingSetup no sean  verdaderos
			if (playersTurn || enemiesMoving || doingSetup)
				//Si uno de estos es verdadero, regresa y no comienza a mover enemigos
				return;

			//Comienza a mover enemigos
			StartCoroutine(MoveEnemies());
		}

		//Llamado por el script Enemy para agregar a sí mismo a la lista de enemigos
		public void AddEnemyToList(Enemy script)
		{
			enemies.Add(script);
		}

		//Cuando el jugador se queda sin puntos de comida
		public void GameOver()
		{
			//Muestra el número de niveles pasados y el mensaje de juego terminado
			levelText.text = "After " + level + " days, you starved.";

			//Activa la imagen negra de fondo gameObject
			levelImage.SetActive(true);

			//Desactiva este GameManager
			enabled = false;
		}

		//Coroutine para mover enemigos en secuencia
		IEnumerator MoveEnemies()
		{
			//Si los enemigos se están moviendo, el jugador no puede moverse
			enemiesMoving = true;

			//Pequeño retraso entre movimientos, para ver los movimientos de los enemigos
			yield return new WaitForSeconds(turnDelay);

			//Si no hay enemigos generados (primer nivel)
			if (enemies.Count == 0)
			{
				//Espera entre movimientos, reemplaza el retraso causado por los enemigos cuando no hay ninguno.
				yield return new WaitForSeconds(turnDelay);
			}

			//Para cada enemigo en la lista de enemigos
			for (int i = 0; i < enemies.Count; i++)
			{
				//Llama a la función MoveEnemy de Enemy en el índice i en la lista de enemigos
				enemies[i].MoveEnemy();

				//Espera el tiempo de movimiento del enemigo antes de mover al siguiente
				yield return new WaitForSeconds(enemies[i].moveTime);
			}
			//Cuando los enemigos hayan terminado de moverse, establezca playersTurn en verdadero para que el jugador pueda moverse
			playersTurn = true;

			//Cuando los enemigos hayan terminado de moverse, establece enemiesMoving en falso
			enemiesMoving = false;
		}

		//Función para iniciar el combate
		public void EnterCombat()
		{
			// Detiene el movimiento del jugador y los enemigos
			playersTurn = false;
			enemiesMoving = false;

			// Establece el estado de combate en verdadero
			isInCombat = true;

			// Muestra la interfaz de combate y comienza el combate
			CombatManager combatManager = FindObjectOfType<CombatManager>();
			if (combatManager != null)
			{
				GameObject combatPanel = combatManager.combatPanel;
				if (combatPanel != null)
				{
					combatPanel.SetActive(true);
					combatManager.StartCombat();
				}
				else
				{
					Debug.LogError("CombatPanel not found in CombatManager!");
				}
			}
			else
			{
				Debug.LogError("CombatManager not found!");
			}
		}
	}
}
