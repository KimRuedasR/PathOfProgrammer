using UnityEngine;
using System;
using System.Collections.Generic;       //Permite usar listas
using Random = UnityEngine.Random;      //Unity Random

namespace Completed

{

	public class BoardManager : MonoBehaviour
	{
		// // Rangos máximo y mínimo de items (muros y comida)
		[Serializable]
		public class Count
		{
			public int minimum; //Valor mínimo
			public int maximum; //Valor máximo


			//Constructor de asignación
			public Count(int min, int max)
			{
				minimum = min;
				maximum = max;
			}
		}


		public int columns = 8; //Columnas
		public int rows = 8; //Filas
		public Count wallCount = new Count(5, 9); //Límite de muros por nivel
		public Count foodCount = new Count(1, 2); //Límite de alimentos por nivel
		public GameObject exit;        //Prefab para generar salida
		public GameObject[] floorTiles; //Array de prefabs de suelo
		public GameObject[] wallTiles; //Array de prefabs de muro
		public GameObject[] foodTiles; //Array de prefabs de comida
		public GameObject[] enemyTiles; //Array de prefabs de enemigos
		public GameObject[] outerWallTiles; //Array de prefabs de bordes
		public AudioClip exitActivationClip; // Clip de audio para la activación de la salida
		private AudioSource audioSource; // Componente de AudioSource

		private Transform boardHolder; //Variable para almacenar una referencia al transform del objeto Board
		private List<Vector3> gridPositions = new List<Vector3>(); //Lista de posibles ubicaciones para colocar los tiles.
		private GameObject exitInstance; // Almacenar la instancia de salida

		private void Start()
		{
			// Obtener o añadir el componente AudioSource
			audioSource = gameObject.AddComponent<AudioSource>();
		}

		//Limpia nuestra lista gridPositions y la prepara para generar un nuevo tablero
		void InitialiseList()
		{
			//Limpiar nuestra lista gridPositions
			gridPositions.Clear();

			//Eje x (columnas).
			for (int x = 1; x < columns - 1; x++)
			{
				//Por columna, largo del eje y (filas)
				for (int y = 1; y < rows - 1; y++)
				{
					//Cada índice añade un nuevo Vector3 a nuestra lista con las coordenadas x e y de esa posición
					gridPositions.Add(new Vector3(x, y, 0f));
				}
			}
		}


		//Configura los muros exteriores y el suelo del tablero
		void BoardSetup()
		{
			//Instancia Board y asigna su transform a boardHolder
			boardHolder = new GameObject("Board").transform;

			//Eje x, desde -1 por la esquina con tiles de suelo o de borde
			for (int x = -1; x < columns + 1; x++)
			{
				//Eje y, comenzando desde -1 para colocar tiles de suelo o de borde
				for (int y = -1; y < rows + 1; y++)
				{
					//Elige un tile aleatorio de nuestro array de prefabs de suelo y prepáralo para instanciar
					GameObject toInstantiate = floorTiles[Random.Range(0, floorTiles.Length)];

					//Verifica si la posición está en el borde, si es así elige un prefab de muro exterior del array
					if (x == -1 || x == columns || y == -1 || y == rows)
						toInstantiate = outerWallTiles[Random.Range(0, outerWallTiles.Length)];

					// Instancia el GameObject usando el prefab elegido para toInstantiate en el Vector3 correspondiente a la posición actual de la cuadrícula en el bucle, y conviértelo a GameObject.
					GameObject instance =
						Instantiate(toInstantiate, new Vector3(x, y, 0f), Quaternion.identity) as GameObject;

					//Establece el padre de nuestro objeto recién instanciado a boardHolder, esto es solo organizativo para evitar desorden en la jerarquía
					instance.transform.SetParent(boardHolder);
				}
			}
		}


		//Devuelve una posición aleatoria de nuestra lista gridPositions
		Vector3 RandomPosition()
		{
			//Declara un entero randomIndex, establece su valor a un número aleatorio entre 0 y el número 
			int randomIndex = Random.Range(0, gridPositions.Count);

			//Declara un Vector3, establece su valor al elemento en randomIndex de nuestra Lista gridPositions.
			Vector3 randomPosition = gridPositions[randomIndex];

			//Elimina el elemento en randomIndex de la lista para que no pueda ser reutilizado
			gridPositions.RemoveAt(randomIndex);

			//Devuelve la posición Vector3 seleccionada aleatoriamente
			return randomPosition;
		}


		//Acepta un array de game objects para elegir junto con un rango mínimo y máximo para el número de objetos a crear
		void LayoutObjectAtRandom(GameObject[] tileArray, int minimum, int maximum)
		{
			//Elige un número aleatorio de objetos  dentro de los límites mínimo y máximo
			int objectCount = Random.Range(minimum, maximum + 1);

			//Instancia objetos hasta que se alcance el límite de objectCount elegido aleatoriamente
			for (int i = 0; i < objectCount; i++)
			{
				//posición aleatoria de la lista de Vector3s disponibles almacenados en gridPosition
				Vector3 randomPosition = RandomPosition();

				//Tile aleatorio de tileArray y asígnalo a tileChoice.
				GameObject tileChoice = tileArray[Random.Range(0, tileArray.Length)];

				//Instancia tileChoice en la posición devuelta por RandomPosition sin cambios en la rotación
				Instantiate(tileChoice, randomPosition, Quaternion.identity);
			}
		}


		//Inicializa nuestro nivel y llama a las funciones anteriores para configurar el tablero de juego
		public void SetupScene(int level)
		{
			////Muros exteriores y el suelo
			BoardSetup();

			//Reinicia nuestra lista de gridpositions
			InitialiseList();

			//Instancia un número aleatorio de tiles de muro basado en los límites mínimo y máximo aleatorio
			LayoutObjectAtRandom(wallTiles, wallCount.minimum, wallCount.maximum);

			//Instancia un número aleatorio de tiles de alimento basado en los límites mínimo y máximo aleaotorio
			LayoutObjectAtRandom(foodTiles, foodCount.minimum, foodCount.maximum);

			// Determina el número de enemigos basado en el número de nivel actual siguiendo un patrón incremental
			int enemyCount = Mathf.Min(1 + (level - 1) / 2, 1 + (level - 1) / 2 + 1);

			//Instancia un número aleatorio de enemigos basado en los límites mínimo y máximo, en posiciones aleatorias
			LayoutObjectAtRandom(enemyTiles, enemyCount, enemyCount);
			// Instancia el tile de salida en la esquina superior derecha de nuestro tablero de juego
			exitInstance = Instantiate(exit, new Vector3(columns - 1, rows - 1, 0f), Quaternion.identity);

			// Desactivar la salida inicialmente
			exitInstance.SetActive(false);
		}

		// Método para activar la salida
		// Método para activar la salida
		public void ActivateExit()
		{
			if (exitInstance != null)
			{
				exitInstance.SetActive(true);

				// Reproducir sonido de activación de la salida
				if (exitActivationClip != null && audioSource != null)
				{
					audioSource.PlayOneShot(exitActivationClip);
				}
			}
		}
	}
}