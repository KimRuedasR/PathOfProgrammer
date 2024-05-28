using UnityEngine;
using System.Collections;

namespace Completed
{
	public class SoundManager : MonoBehaviour
	{
		public AudioSource efxSource; //Arrastra una referencia al audio source que reproducirá los efectos de sonido
		public AudioSource musicSource; //Arrastra una referencia al audio source que reproducirá la música
		public static SoundManager instance = null; //Permite que otros scripts llamen funciones de SoundManager.				
		public float lowPitchRange = .95f; //Tono más bajo aplicado aleatoriamente a un efecto de sonido
		public float highPitchRange = 1.05f; //Tono más alto aplicado aleatoriamente a un efecto de sonido


		void Awake()
		{
			//Verifica si ya existe una instancia de SoundManager
			if (instance == null)
				instance = this;
			else if (instance != this)
				// Destruye este objeto, asegura patrón singleton para que una sola instancia de SoundManager.
				Destroy(gameObject);

			//Configura SoundManager para que no se destruya al cargar una nueva escena.
			DontDestroyOnLoad(gameObject);
		}


		//Reproduce el clip
		public void PlaySingle(AudioClip clip)
		{
			//Clip de nuestro efxSource al clip pasado como parámetro
			efxSource.clip = clip;

			//Play
			efxSource.Play();
		}


		// RandomizeSfx elige aleatoriamente entre varios clips de audio y cambia ligeramente su tono
		public void RandomizeSfx(params AudioClip[] clips)
		{
			//Genera un número aleatorio entre 0 y la longitud de nuestro array de clips pasados como parámetro
			int randomIndex = Random.Range(0, clips.Length);

			//Elige un tono aleatorio para reproducir nuestro clip entre nuestros rangos de tono alto y bajo
			float randomPitch = Random.Range(lowPitchRange, highPitchRange);

			//Tono del audio source al tono elegido aleatoriamente
			efxSource.pitch = randomPitch;

			//Clip al clip en nuestro índice elegido aleatoriamente
			efxSource.clip = clips[randomIndex];

			//Play
			efxSource.Play();
		}
	}
}
