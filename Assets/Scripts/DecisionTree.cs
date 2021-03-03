using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Newtonsoft.Json;

public partial class CountriesScheme {
	[JsonProperty("countries")]
	public string[] array;
}

namespace DecisionTreeScheme {    
	public partial class Root {
		public List<Action> actions;
	}

	public partial class Action {
		public string title;
		public Dictionary<int, float> infect;
		public string question;
		public Dictionary<string, int> answers;
	}
}

public class DecisionTree : MonoBehaviour
{
	public TextAsset jsonEvents;

	public InfectEarth infect;
	public OrbitEarth orbit;

	public TMPro.TextMeshProUGUI TitleText;
	public TMPro.TextMeshProUGUI QuestionText;
	public Transform ButtonContainer;
	public GameObject ButtonPrefab;

	private List<DecisionTreeScheme.Action> actions;

	private void OnEnable()
	{
		actions = JsonConvert.DeserializeObject<DecisionTreeScheme.Root>(jsonEvents.text).actions;		
		DisplayAction(0);
	}

	public void DisplayAction(int actionID) {
		DecisionTreeScheme.Action action = actions[actionID];

		// Infect Countries
		foreach (int i in action.infect.Keys) {
			infect.InfectCountry(i, action.infect[i]);
		}

		// Rotate camera
		switch (actionID) {
			case 0:
				orbit.SetView(new Vector3(1.75f, 0.6f, 7.5f));
				break;
			case 9:
				orbit.SetView(new Vector3(0.22f, 0.725f, 6f));
				break;
			case 18:
				orbit.SetView(new Vector3(-1.7f, 0.675f, 7.5f));
				break;
		}

		// Display action title, question & button text
		TitleText.text = action.title;
		QuestionText.text = action.question;

		foreach (Transform button in ButtonContainer) Destroy(button.gameObject); // Destroy all previous buttons

		if (action.answers.Keys.Count > 0) {
			foreach (string key in action.answers.Keys) {
				GameObject button = Instantiate(ButtonPrefab, ButtonContainer); // Create new Button in Button Comtainer

				TMPro.TextMeshProUGUI ButtonText = button.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>();
				int resultingAction = action.answers[key];

				ButtonText.text = key;
				button.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate () {  // Call this method recursively on click
					DisplayAction(resultingAction);
				});
			} 
		} else {
			GameObject button = Instantiate(ButtonPrefab, ButtonContainer);
			button.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "Обратно към главното меню";
			button.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate () {  // Call this method recursively on click
				SceneManager.LoadScene("Menu");
			});
		}
	}

	/*
	public void DisplayAction(int actionID) {
		var action_reaction = tree.action_reaction[actionID]; //TODO: check if it's being passed by reference

		string currentAction = tree.language[language].actions[actionID];

		Debug.Log("Action: " + currentAction); //TODO: temp
		actionText.text = currentAction; // Set action text

		foreach (Transform button in ButtonContainer) Destroy(button.gameObject);// Destroy all previous buttons

		if (action_reaction.Keys.Count > 0) {
			foreach (int reactionID in action_reaction.Keys) {
				string possibleReaction = tree.language[language].reactions[reactionID];
				int resultingAction = action_reaction[reactionID];

				Debug.Log(possibleReaction + ": " + resultingAction); //TODO: temp
				GameObject button = Instantiate(ButtonPrefab, ButtonContainer); // Create new Button
				button.transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().text = possibleReaction; //Set the text of the new button
				button.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate () {  // Call this method recursively when on click
					DisplayAction(resultingAction);
				});
			}
		} else {
			GameObject button = Instantiate(ButtonPrefab, ButtonContainer);
			button.transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().text = "\u21BB"; //TODO: make available in many languages (a whole new menu necessary)
			button.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate () {  // Call this method recursively when on click
				DisplayAction(0);
			});
		}
	}
	*/
}