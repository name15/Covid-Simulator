using UnityEngine;

public class InfectEarth : MonoBehaviour
{
	const int countryCount = 177; //WARNING: must be manually set

	public Material EarthMaterial;
	public TextAsset jsonCountries;

	public string[] countries;

	private float[] infectionLevel = new float[countryCount];

	public void InfectCountry(int countryId, float value) {
		infectionLevel[countryId] = value;
		EarthMaterial.SetFloatArray("_InfectionStatus", infectionLevel);
	}
}
