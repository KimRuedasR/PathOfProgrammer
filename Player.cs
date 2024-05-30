using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI; //Usar UI.

namespace Completed
{
	//Player hereda de MovingObject, nuestra clase base para objetos que pueden moverse. Enemy también hereda de esta
	public class Player : MovingObject
	{
		public float restartLevelDelay = 1f; //Tiempo de demora en segundos para reiniciar el nivel
		public int pointsPerFood = 1; //Número de puntos para agregar al jugador cuando recoge un objeto de comida
		public int pointsPerSoda = 1; //Número de puntos para agregar al jugador cuando recoge un objeto de soda
		public int wallDamage = 1; //Daño que un jugador hace a un muro al atacarlo
		public Text foodText; //UI para mostrar el total de puntos de comida del jugador.
		public AudioClip moveSound1; //1 Audio de movimiento
		public AudioClip moveSound2; //2 Audio de movimiento
		public AudioClip eatSound1; //1 Audio de comida
		public AudioClip eatSound2; //2 Audio de comida
		public AudioClip drinkSound1; //1 Audio de soda.
		public AudioClip drinkSound2; //2 Audio de soda
		public AudioClip gameOverSound; //Audio de muerte
		private Animator animator; //almacenar una referencia al componente Animator del jugador
		private int food; //Almacenar el total de puntos de comida del jugador durante el nivel
		private const int maxFood = 4; // Maximo de puntos de comida que el jugador puede obtener en el juego
		private bool facingRight = true; // Almacenar la direccion del jugador
				
		public Image FoodImage; // El objeto de la imagen de vida en el UI
		public Sprite life4Sprite;
		public Sprite life3Sprite;
		public Sprite life2Sprite;
		public Sprite life1Sprite;
		public Sprite life0Sprite;
		

		//Start sobrescribe la función Start de MovingObject
		protected override void Start()
		{
			//Obtén una referencia al componente Animator del jugador
			animator = GetComponent<Animator>();

			//Obtén el total de puntos de comida en GameManager.instance entre niveles
			food = GameManager.instance.playerFoodPoints;

			//Muestra el texto con el total de comida del jugador
			foodText.text = "Vida: " + food;

			//Llama a la función Start de la clase base MovingObject
			base.Start();

			//Inicializa la imagen de vida
			UpdateFoodImage();
		}


		//Esta función se llama cuando el comportamiento se desactiva o se vuelve inactivo
		private void OnDisable()
		{
			//Cuando el objeto Player se desactiva, almacena el total de comida actual en GameManager para que pueda recargarse en el próximo nivel
			GameManager.instance.playerFoodPoints = food;
		}

		private void UpdateFoodImage()
		{
			if (food == 4)
			{
				FoodImage.sprite = life4Sprite;
			}
			else if (food == 3)
			{
				FoodImage.sprite = life3Sprite;
			}
			else if (food == 2)
			{
				FoodImage.sprite = life2Sprite;
			}
			else if (food == 1)
			{
				FoodImage.sprite = life1Sprite;
			}
			else
			{
				FoodImage.sprite = life0Sprite;
			}
		}


		private void Update()
		{
			// Si el juego está en combate, no permite el movimiento del jugador
			if (GameManager.instance.isInCombat) return;

			//Si no es el turno del jugador, sale de la función
			if (!GameManager.instance.playersTurn) return;

			//Dirección de movimiento en x y y
			int horizontal = 0;
			int vertical = 0;
			
			//Input manager, redondea a entero y almacena en horizontal para dirección de movimiento en x 
			//Obtén la entrada del gestor de entrada, 
			
			horizontal = (int) (Input.GetAxisRaw ("Horizontal"));
			
			//Input manager, redondea a entero y almacena en vertical para dirección de movimiento en y
			vertical = (int) (Input.GetAxisRaw ("Vertical"));
			
			 //Movimiento horizontalmente, establece vertical en cero.
			if(horizontal != 0)
			{
				vertical = 0;
			}

			//Verifica si tenemos un valor distinto de cero para horizontal o vertical.
			if (horizontal != 0 || vertical != 0)
			{

				// Verificar la dirección del movimiento y voltear al jugador si es necesario
        		Flip(horizontal);
				//Wall como parámetro para que el jugador interactue (ataque)
				//horizontal y vertical como parámetros para dirección de movimiento
				AttemptMove<Wall>(horizontal, vertical);
			}
		}
		
		//Método para voltear la dirección del jugador
			private void Flip(int horizontal)
			{
				// Si el jugador se mueve a la derecha y no está mirando a la derecha, voltear
				if (horizontal > 0 && !facingRight)
				{
					facingRight = true;
					Vector3 theScale = transform.localScale;
					theScale.x *= -1;
					transform.localScale = theScale;
				}
				// Si el jugador se mueve a la izquierda y está mirando a la derecha, voltear
				else if (horizontal < 0 && facingRight)
				{
					facingRight = false;
					Vector3 theScale = transform.localScale;
					theScale.x *= -1;
					transform.localScale = theScale;
				}
			}


		//Sobrescribe la función AttemptMove en la clase MovingObject.
		// AttemptMove toma parámetro para que el jugador interactue y parámetros para dirección de movimiento
		protected override void AttemptMove<T>(int xDir, int yDir)
		{
			//Actualiza el texto de la comida para reflejar el cambio
			foodText.text = "Vida: " + food;

			//AttemptMove de la clase base, pasando el componente T (Wall) y la dirección x e y
			base.AttemptMove<T>(xDir, yDir);

			//Referenciar el resultado del Linecast hecho en Move
			RaycastHit2D hit;

			//Si el movimiento es exitoso, reproduce el sonido de movimiento
			if (Move(xDir, yDir, out hit))
			{
				//RandomizeSfx de SoundManager para reproducir el sonido de movimiento
				SoundManager.instance.RandomizeSfx(moveSound1, moveSound2);
			}

			//Verifica si el juego ha terminado
			CheckIfGameOver();

			//Turno del jugador en falso por movimiento
			GameManager.instance.playersTurn = false;
		}


		//OnCantMove sobrescribe la función OnCantMove en MovingObject
		//Toma un parametro(Wall) para que el jugador interactue
		protected override void OnCantMove<T>(T component)
		{
			//Establece hitWall para que sea igual al componente pasado como parámetro
			Wall hitWall = component as Wall;

			// Llama a la función DamageWall del Wall que estamos golpeando.
			hitWall.DamageWall(wallDamage);

			//Trigger de ataque del controlador de animación del jugador para reproducir la animación de ataque
			animator.SetTrigger("playerChop");
		}


		//OnTriggerEnter2D se envía cuando otro objeto entra en un collider adjunto a este objeto (física 2D).
		private void OnTriggerEnter2D(Collider2D other)
		{

			//Revisa si la etiqueta con la que colisionó es Exit
			if (other.tag == "Exit")
			{
				//Función Restart para comenzar el siguiente nivel con un retraso de restartLevelDelay (1 segundo).
				Invoke("Restart", restartLevelDelay);

				//Desactiva el objeto del jugador ya que el nivel ha terminado
				enabled = false;
			}

			//Revisa si la etiqueta con la que colisionó es Food
			else if (other.tag == "Food")
			{
				//Añade pointsPerFood al total actual de comida del jugador
				if (food < maxFood) {
					food += pointsPerFood;

					//Actualiza foodText para representar el total actual y notifica al jugador que ha ganado puntos
					foodText.text = "+" + pointsPerFood + " Food: " + food;

					//Actualiza la imagen de la vida del jugador
                	UpdateFoodImage();

					//RandomizeSfx de SoundManager y pasa dos sonidos de comer
					SoundManager.instance.RandomizeSfx(eatSound1, eatSound2);
				}
				//Desactiva el objeto de comida con el que colisionó el jugador
				other.gameObject.SetActive(false);
			}

			//Revisa si la etiqueta con la que colisionó es Soda
			else if (other.tag == "Soda")
			{
				//Añade pointsPerSoda al total actual de comida del jugador
				if (food < maxFood) {
					food += pointsPerSoda;

					//Actualiza foodText para representar el total actual y notifica al jugador que ha ganado puntos
					foodText.text = "+" + pointsPerSoda + " Food: " + food;

					//Actualiza la imagen de la vida del jugador
                	UpdateFoodImage();

					//RandomizeSfx de SoundManager y pasa dos sonidos de tomar
					SoundManager.instance.RandomizeSfx(drinkSound1, drinkSound2);
				}
				//Desactiva el objeto de bebida con el que colisionó el jugador
				other.gameObject.SetActive(false);
			}
		}


		//Restart recarga la escena cuando se llama
		private void Restart()
		{
			//Carga la última escena cargada (Main), la única escena en el juego.
			//Modo "Single" para que reemplace la existente y no cargue todos los objetos de la escena en la escena actual.
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
		}


		//LoseFood se llama cuando un enemigo ataca al jugador
		//Toma un parámetro loss que especifica cuántos puntos perder
		public void LoseFood(int loss)
		{
			//Establece el disparador para el animador del jugador para la transición a la animación playerHit
			animator.SetTrigger("playerHit");

			//Resta los puntos de comida perdidos del total de puntos de comida del jugador
			food -= loss;

			//Actualiza la pantalla de comida con el nuevo total
			foodText.text = "-" + loss + " Food: " + food;

			//Actualiza la imagen de la vida del jugador
        	UpdateFoodImage();

			//Verifica si el juego ha terminado
			CheckIfGameOver();
		}


		//Verifica si al jugador le quedan puntos de comida y si es así, termina el juego
		private void CheckIfGameOver()
		{
			//Verifica si el total de puntos de comida es menor o igual a cero
			if (food <= 0)
			{
				//Llama la función PlaySingle de SoundManager y pasa el gameOverSound como el clip de audio para reproducir
				SoundManager.instance.PlaySingle(gameOverSound);

				//Detiene la música de fondo.
				SoundManager.instance.musicSource.Stop();

				//Llama a la función GameOver de GameManager.
				GameManager.instance.GameOver();
			}
		}
	}
}

