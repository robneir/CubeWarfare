using UnityEngine;
using System.Collections;

[RequireComponent (typeof(Pickable))]
public class AmmoBox : MonoBehaviour {

	Pickable thisP;
	public int Ammo = 32;
	public AudioClip getNoise;
	int howMuchAmmo = 0;
	WeaponShoot whichWeapon;

	public void SetHowMuch(int hm, WeaponShoot ww)
	{
		howMuchAmmo = hm;
		whichWeapon = ww;
	}
	// Use this for initialization
	void Start () {
		thisP = GetComponent<Pickable> ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void Hover()
	{
		thisP.pickText = "Press \"E\" to restock. (" + Ammo.ToString () + " left)";
	}

	public void Use()
	{
		if (this.Ammo > 0 && whichWeapon.weapon.ammoStockpile == whichWeapon.weapon.maxAmmo)
			return;

		if(this.Ammo >= howMuchAmmo)
		{
			if(whichWeapon.weapon.ammoStockpile + howMuchAmmo <= whichWeapon.weapon.maxAmmo)
			{
				this.Ammo -= howMuchAmmo;
				whichWeapon.weapon.ammoStockpile += howMuchAmmo;
			}
			else
			{
				this.Ammo -= (whichWeapon.weapon.maxAmmo - whichWeapon.weapon.ammoStockpile);
				whichWeapon.weapon.ammoStockpile = whichWeapon.weapon.maxAmmo;
			}
		}
		else
		{
			if(whichWeapon.weapon.ammoStockpile + this.Ammo <= whichWeapon.weapon.maxAmmo)
			{
				this.Ammo = 0;
				whichWeapon.weapon.ammoStockpile += this.Ammo;
			}
			else
			{
				this.Ammo -= (whichWeapon.weapon.maxAmmo - whichWeapon.weapon.ammoStockpile);
				whichWeapon.weapon.ammoStockpile = whichWeapon.weapon.maxAmmo;
			}
		}

		AudioSource.PlayClipAtPoint (getNoise, this.transform.position);

		if(Ammo <= 0)
		{
			GameObject.Destroy(this.gameObject);
		}
	}
}
