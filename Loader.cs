using UnityEngine;
using System.Collections;

namespace Completed
{
	public class Loader : MonoBehaviour
	{
		public GameObject gameManager; //Prefab de SoundManager para instanciar		
		public GameObject soundManager; //Prefab de GameManager para instanciar


		void Awake()
		{
			//Revisa si un GameManager ya ha sido asignado a la variable GameManager.instance o si es nulo
			if (GameManager.instance == null)

				//Instancia el prefab gameManager
				Instantiate(gameManager);

			//Revisa si un SoundManager ya ha sido asignado a la variable SoundManager.instance o si es nulo
			if (SoundManager.instance == null)

				//Instancia el prefab soundManager
				Instantiate(soundManager);
		}
	}
}