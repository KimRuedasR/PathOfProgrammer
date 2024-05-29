using UnityEngine;
using System.Collections;

namespace Completed
{
	//Abstract permite crear clases incompletas y deben implementarse en una clase derivada.
	public abstract class MovingObject : MonoBehaviour
	{
		public float moveTime = 0.1f; //Tiempo de movimiento en segundos
		public LayerMask blockingLayer; //Capa en la que se verifica la colisión


		private BoxCollider2D boxCollider; //Componente BoxCollider2D
		private Rigidbody2D rb2D; //Componente Rigidbody2D
		private float inverseMoveTime; //movimiento más eficiente
		private bool isMoving; //Si el objeto se está moviendo actualmente


		//Funciones protegidas y virtuales pueden ser sobrescritas por clases derivadas
		protected virtual void Start()
		{
			//Obtiene una referencia al componente BoxCollider2D
			boxCollider = GetComponent<BoxCollider2D>();

			///Obtiene una referencia al componente Rigidbody2D
			rb2D = GetComponent<Rigidbody2D>();

			//Al almacenar el recíproco del tiempo de movimiento, se usa multiplicando en lugar de dividir, más eficiente.
			inverseMoveTime = 10f / moveTime;
		}


		// Move devuelve true si es capaz de moverse y false si no.
		// Toma parámetros para la dirección x, dirección y y un RaycastHit2D para verificar la colisión.
		protected bool Move(int xDir, int yDir, out RaycastHit2D hit)
		{
			//Almacena la posición inicial, basada en la posición del transform.
			Vector2 start = transform.position;

			//Calcula la posición final basada en los parámetros
			Vector2 end = start + new Vector2(xDir, yDir);

			//Desactiva el boxCollider para que el linecast no golpee a este objeto
			boxCollider.enabled = false;

			//Lanza un rayo desde el punto de inicio hasta el punto final, verificando la colisión en la capa
			hit = Physics2D.Linecast(start, end, blockingLayer);

			//Reactiva el boxCollider después del linecast
			boxCollider.enabled = true;

			//Revisa si nada fue golpeado y que el objeto no se está moviendo
			if (hit.transform == null && !isMoving)
			{
				//Co-routina que mueve las unidades de un espacio a otro
				StartCoroutine(SmoothMovement(end));

				//Regresa true, movimiento exitoso
				return true;
			}

			//Si algo fue golpeado, regresa false, movimiento fallido
			return false;
		}


		//Co-rutina para mover unidades de un espacio a otro, parámetro final para especificar a dónde moverse
		protected IEnumerator SmoothMovement(Vector3 end)
		{
			//El objeto ahora se está moviendo
			isMoving = true;

			/*
			
			Calcula la distancia restante para moverse basada en la magnitud cuadrada de la diferencia entre la posición actual y el parámetro final

			La magnitud cuadrada se usa en lugar de la magnitud porque es computacionalmente más eficiente	
			
			Como en el Quake lol
			 */

			float sqrRemainingDistance = (transform.position - end).sqrMagnitude;

			//Mientras que la distancia restante sea mayor que una cantidad muy pequeña (Epsilon):
			while (sqrRemainingDistance > float.Epsilon)
			{
				//Encuentra una nueva posición proporcionalmente más cercana al final, basada en el moveTime
				Vector3 newPostion = Vector3.MoveTowards(rb2D.position, end, inverseMoveTime * Time.deltaTime);

				//Mueve el objeto a la nueva posición
				rb2D.MovePosition(newPostion);

				//Recalcula la distancia restante después de moverse
				sqrRemainingDistance = (transform.position - end).sqrMagnitude;

				//Regresa y repite hasta que sqrRemainingDistance sea cercano a cero para terminar la función
				yield return null;
			}

			//Asegura que el objeto esté al final de su movimiento
			rb2D.MovePosition(end);

			//El objeto no se mueve
			isMoving = false;
		}


		//La palabra 'virtual' significa que AttemptMove puede ser anulada por clases heredadas usando la palabra clave override.
		//AttemptMove toma un parámetroespecificar el tipo de componente con el qla unidad interactúa si está bloqueada (jugador y enemigos, pared y jugador).
		protected virtual void AttemptMove<T>(int xDir, int yDir)
			where T : Component
		{
			//Almacena la información sobre lo que golpea el linecast cuando se llama a Move
			RaycastHit2D hit;

			//El método Move devuelve true si es capaz de moverse y false si no
			bool canMove = Move(xDir, yDir, out hit);

			//Revisa si algo fue golpeado por la línea de lanzamiento
			if (hit.transform == null)
				//If nothing was hit, return and don't execute further code.
				return;


			//Componente de tipo T adjunto al objeto golpeado
			T hitComponent = hit.transform.GetComponent<T>();


			//si canMove es falso y hitComponent no es nulo MovingObject está bloqueado y ha golpeado algo con lo que puede interactuar
			if (!canMove && hitComponent != null)
				//llama a la función OnCantMove y pasa hitComponent como parámetro
				OnCantMove(hitComponent);
		}


		//El modificador 'abstract' indica que lo que se está modificando tiene una implementación faltante o incompleta
		//onCantMove será sobreescrito por funciones en las clases heredadas
		protected abstract void OnCantMove<T>(T component)
			where T : Component;
	}
}
