using UnityEngine;
using System.Collections;

namespace Completed
{
	public class Wall : MonoBehaviour
	{
		public AudioClip chopSound1; //Clip de ataque del jugador 1
		public AudioClip chopSound2; //Clip de ataque del jugador 2
		public Sprite dmgSprite; //Cambiar sprite cuando el muro es dañado
		public int hp = 3; //Vida del muro


		private SpriteRenderer spriteRenderer; //Referencia al SpriteRenderer adjunnnto al GameObject


		void Awake()
		{
			//Obtiene una referencia al componente SpriteRenderer adjunto a este GameObject
			spriteRenderer = GetComponent<SpriteRenderer>();
		}


		//Cuando el jugador ataca un muro
		public void DamageWall(int loss)
		{
			//Funcion RandomizeSfx de SoundManager para reproducir uno de los dos sonidos
			SoundManager.instance.RandomizeSfx(chopSound1, chopSound2);

			//Cambiar el sprite del muro a dmgSprite
			spriteRenderer.sprite = dmgSprite;

			//Restar puntos de vida al muro
			hp -= loss;

			//Verificar si la vida del muro es menor o igual a 0 desactivar el GameObject
			if (hp <= 0)
				gameObject.SetActive(false);
		}
	}
}
