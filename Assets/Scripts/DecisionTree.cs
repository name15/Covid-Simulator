using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DecisionTreeScheme {    
	public partial class Root {
		[JsonProperty("action-reaction")]
		public Dictionary<int, int>[] action_reaction;

		public Dictionary<string, Language> language;
	}

	public partial class Language {
		public string[] actions;

		public string[] reactions;
	}
}

public class DecisionTree : MonoBehaviour
{
	public TextAsset jsonFile;
	public GameObject QuestionText;
	public Transform ButtonContainer;
	public GameObject ButtonPrefab;

	private string language = "en"; //TODO: make public (with checks)
	private DecisionTreeScheme.Root tree;
	private UnityEngine.UI.Text actionText;

	private void Start()
	{
		tree = JsonConvert.DeserializeObject<DecisionTreeScheme.Root>(jsonFile.text);
		actionText = QuestionText.GetComponent<UnityEngine.UI.Text>();

		DisplayAction(0);
	}

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
}