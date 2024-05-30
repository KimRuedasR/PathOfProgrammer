using UnityEngine;
using TMPro; // Import TextMeshPro namespace
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using System.IO;

namespace Completed
{
    public class CombatManager : MonoBehaviour
    {
        public GameObject combatPanel; // Panel de combate
        public TMP_Text questionText; // Texto de la pregunta
        public Button[] answerButtons; // Botones para las respuestas

        public Sprite correctSprite; // Sprite dorado
        public Sprite incorrectSprite; // Sprite rojo

        public AudioClip combatStartClip; // Clip de audio para el inicio del combate
        public AudioClip enemyDestroyedClip; // Clip de audio para la destrucción del enemigo
        public AudioClip playerChopClip; // Clip de audio para la animación de corte del jugador
        private AudioSource audioSource; // Componente de AudioSource

        [System.Serializable]
        public struct Question
        {
            public string question;
            public string[] answers;
            public int correctAnswerIndex;
        }

        public List<Question> questions; // Lista de preguntas

        private void Start()
        {
            // Asegúrate de que el panel de combate esté inactivo al inicio
            combatPanel.SetActive(false);

            // Cargar preguntas desde el archivo JSON
            LoadQuestionsFromJson();

            // Obtener o añadir el componente AudioSource
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        private void LoadQuestionsFromJson()
        {
            TextAsset jsonFile = Resources.Load<TextAsset>("questions");
            questions = JsonUtility.FromJson<QuestionList>(jsonFile.text).questions;
        }

        public void StartCombat()
        {
            // Reproducir sonido de inicio de combate
            PlaySound(combatStartClip);

            // Seleccionar una pregunta aleatoria y mostrarla
            int randomIndex = Random.Range(0, questions.Count);
            Question selectedQuestion = questions[randomIndex];
            DisplayQuestion(selectedQuestion);
        }

        private void DisplayQuestion(Question question)
        {
            // Mostrar el texto de la pregunta
            questionText.text = question.question;

            // Mostrar las respuestas en los botones
            for (int i = 0; i < answerButtons.Length; i++)
            {
                if (i < question.answers.Length)
                {
                    answerButtons[i].GetComponentInChildren<TMP_Text>().text = question.answers[i];
                    answerButtons[i].gameObject.SetActive(true);

                    // Añadir listener al botón de respuesta
                    int index = i; // Captura la variable del índice
                    answerButtons[i].onClick.RemoveAllListeners();
                    answerButtons[i].onClick.AddListener(() => CheckAnswer(index, question.correctAnswerIndex));
                }
                else
                {
                    answerButtons[i].gameObject.SetActive(false);
                }
            }

            // Activar el panel de combate
            combatPanel.SetActive(true);
        }

        private void CheckAnswer(int selectedAnswerIndex, int correctAnswerIndex)
        {
            if (selectedAnswerIndex == correctAnswerIndex)
            {
                // Si la respuesta es correcta, cambiar el sprite a dorado
                StartCoroutine(ShowAnswerFeedback(answerButtons[selectedAnswerIndex], correctSprite));
                // Si la respuesta es correcta, el jugador gana
                Debug.Log("Correct answer! Player wins!");

                // Reproduce la animación de ataque del jugador y sonido de corte
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                Animator playerAnimator = player.GetComponent<Animator>();
                playerAnimator.SetTrigger("playerChop");
                PlaySound(playerChopClip);

                // Destruye al enemigo y reproduce sonido de destrucción
                GameObject enemy = GameManager.instance.GetCurrentEnemy();
                if (enemy != null)
                {
                    Enemy enemyScript = enemy.GetComponent<Enemy>();
                    GameManager.instance.RemoveEnemy(enemyScript);
                    Destroy(enemy);
                    PlaySound(enemyDestroyedClip);
                }

                // Desactivar el panel de combate
                StartCoroutine(DisableCombatPanelAfterDelay());
            }
            else
            {
                // Si la respuesta es incorrecta, cambiar el sprite a rojo
                StartCoroutine(ShowAnswerFeedback(answerButtons[selectedAnswerIndex], incorrectSprite));
                // Si la respuesta es incorrecta, el enemigo ataca
                Debug.Log("Wrong answer! Enemy attacks!");

                // Reproduce la animación de ataque del enemigo
                GameObject enemy = GameManager.instance.GetCurrentEnemy();
                if (enemy != null)
                {
                    Animator enemyAnimator = enemy.GetComponent<Animator>();
                    enemyAnimator.SetTrigger("enemyAttack");

                    // El jugador pierde puntos de vida
                    GameObject player = GameObject.FindGameObjectWithTag("Player");
                    Player playerScript = player.GetComponent<Player>();
                    playerScript.LoseFood(1);
                }

                // Desactivar el panel de combate
                StartCoroutine(DisableCombatPanelAfterDelay());
            }

            // Salir del estado de combate
            GameManager.instance.isInCombat = false;
        }

        private IEnumerator DisableCombatPanelAfterDelay()
        {
            // Esperar 1 segundo
            yield return new WaitForSeconds(0.5f);
            // Desactivar el panel de combate
            combatPanel.SetActive(false);
        }

        private IEnumerator ShowAnswerFeedback(Button button, Sprite feedbackSprite)
        {
            // Guardar el sprite original
            Sprite originalSprite = button.GetComponent<Image>().sprite;
            // Cambiar al sprite de feedback
            button.GetComponent<Image>().sprite = feedbackSprite;
            // Esperar 2 segundos
            yield return new WaitForSeconds(0.5f);
            // Restaurar el sprite original
            button.GetComponent<Image>().sprite = originalSprite;
        }

        private void PlaySound(AudioClip clip)
        {
            if (clip != null && audioSource != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }

        [System.Serializable]
        private class QuestionList
        {
            public List<Question> questions;
        }
    }
}