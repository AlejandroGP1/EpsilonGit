using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuButtons : MonoBehaviour
{

    public void LoadGameScene()
	{
		SceneManager.LoadScene("SampleScene");
	}

    public void LoadGameScene1()
	{
		SceneManager.LoadScene("SampleScene 1");
	}

    public void LoadGameScene2()
	{
		SceneManager.LoadScene("SampleScene 2");
	}

}

// En un futuro me gustaría añadir sonido de cartas a los botones de los diferentes menus.
// El problema es que la acción de cada botón interrumpe el sonido de la carta
// Para arreglar esto he considerado poner un contador de unas decimas de segundo para que
//al sonido le de tiempo a reprodusirse
// Esto tambien podría estar acompañado de una animación de transición mientras se
//reproduce el sonido