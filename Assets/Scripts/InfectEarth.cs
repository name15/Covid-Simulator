using UnityEngine;
using Newtonsoft.Json;

public class InfectEarth : MonoBehaviour
{
	const int countryCount = 177;

	public Material EarthMaterial;
	public TextAsset jsonCountries;

	[Range(0, countryCount - 1)]
	public int id; //TODO: TEMP
	public string[] countries;

	private float[] infectionStatus = new float[countryCount];

	public void InfectCountry(int countryId, float value) {
		infectionStatus[countryId] = value;
		EarthMaterial.SetFloatArray("_InfectionStatus", infectionStatus);
	}

	private void Start() {
		countries = JsonConvert.DeserializeObject<CountriesScheme>(jsonCountries.text).array;
	}

	private void OnValidate() { //TODO: TEMP
		infectionStatus = new float[countryCount];
		InfectCountry(id, 1);
	}
}
