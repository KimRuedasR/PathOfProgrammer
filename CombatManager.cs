using UnityEngine;
using TMPro; // Import TextMeshPro namespace
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;

namespace Completed
{
    public class CombatManager : MonoBehaviour
    {
        public GameObject combatPanel; // Panel de combate
        public TMP_Text questionText; // Texto de la pregunta
        public Button[] answerButtons; // Botones para las respuestas

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
        }

        private void LoadQuestionsFromJson()
        {
            TextAsset jsonFile = Resources.Load<TextAsset>("questions");
            questions = JsonUtility.FromJson<QuestionList>(jsonFile.text).questions;
        }

        public void StartCombat()
        {
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
                // Si la respuesta es correcta, el jugador gana
                Debug.Log("Correct answer! Player wins!");
                // Desactivar el panel de combate
                combatPanel.SetActive(false);
                // Aquí puedes añadir más lógica para que el enemigo sea destruido
            }
            else
            {
                // Si la respuesta es incorrecta, el enemigo ataca
                Debug.Log("Wrong answer! Enemy attacks!");
                // Desactivar el panel de combate
                combatPanel.SetActive(false);
                // Aquí puedes añadir más lógica para que el enemigo ataque al jugador
            }

            // Salir del estado de combate
            GameManager.instance.isInCombat = false;
        }

        [System.Serializable]
        private class QuestionList
        {
            public List<Question> questions;
        }
    }
}
