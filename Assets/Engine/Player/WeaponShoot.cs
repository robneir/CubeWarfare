
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class WeaponShoot : MonoBehaviour {

	public GameObject weaponStart;
	public GameObject weaponSprint;
	public Weapon weapon;
	Vector3 initPos;

	bool isFiring = false;
	private AudioSource myAudioSource;
	public Image reticle;
	GravityFPSWalker gfps;

	bool zooming = false;
	public Camera gunCam;
	public Camera worldCam;
	private float FOVinit;
	private float FOVsprint;
	private Animator animator;

	
	public bool sprinting
	{
		get { return gfps.sprinting;}
	}

	private Text interaction;
	// Use this for initialization
	void Start () 
	{
		gfps = this.transform.root.GetComponent<GravityFPSWalker> ();
		FOVinit = worldCam.fieldOfView;
		FOVsprint = FOVinit + 5;
		animator = transform.GetChild (0).GetComponent<Animator> ();

		interaction = GameObject.Find ("Canvas").transform.FindChild ("Interaction").GetComponent<Text> ();
		myAudioSource = GetComponent<AudioSource> ();
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(Input.GetKey(KeyCode.R))
		{
			if(weapon.ammoStockpile > 0 && weapon.currentClip != weapon.ClipSize)
				StartReloading();
		}
		if(Input.GetMouseButtonDown(1))
		{
			zooming = !zooming;
			reticle.gameObject.active = !zooming;
			gfps.SetZooming(zooming);
		}

		if(weapon.Automatic)
		{
			if (Input.GetMouseButton (0)) 
			{
				isFiring = true;
				weapon.FireRate -= Time.deltaTime;

				if(weapon.FireRate < 0)
				{
					if(weapon.currentClip > 0 && !reloading)
						FireBullet ();
					else if (weapon.ammoStockpile > 0)
						StartReloading();
				} 
			}
			else
			{
				isFiring = false;
			}
		}
		else
		{
			if (Input.GetMouseButtonDown(0)) 
			{
				if(weapon.currentClip > 0 && !reloading)
					FireBullet ();
				else if (weapon.ammoStockpile > 0)
					StartReloading();
			}
			else
			{
				isFiring = false;
			}
		}
		if(Input.GetMouseButtonUp(0))
		{
			weapon.FireRate = 0;
		}
		if(sprinting)
		{
			this.transform.position = Vector3.Lerp(this.transform.position, weaponSprint.transform.position, .1f);
			this.transform.rotation = Quaternion.Lerp(this.transform.rotation, weaponSprint.transform.rotation, .1f);
			
			gunCam.fieldOfView = Mathf.Lerp(gunCam.fieldOfView, FOVsprint, .1f);
			worldCam.fieldOfView = Mathf.Lerp(worldCam.fieldOfView, FOVsprint, .1f);
		}
		else if(zooming)
		{
			this.transform.position = Vector3.Lerp(this.transform.position, weapon.weaponADS.transform.position, .1f);
			this.transform.rotation = Quaternion.Lerp(this.transform.rotation, weapon.weaponADS.transform.rotation, .1f);

			gunCam.fieldOfView = Mathf.Lerp(gunCam.fieldOfView, weapon.FOVzoom, .1f);
			worldCam.fieldOfView = Mathf.Lerp(worldCam.fieldOfView, weapon.FOVzoom, .1f);
		}
		else
		{
			this.transform.position = Vector3.Lerp(this.transform.position, weaponStart.transform.position, .12f);
			this.transform.rotation = Quaternion.Lerp(this.transform.rotation, weaponStart.transform.rotation, .1f);

			gunCam.fieldOfView = Mathf.Lerp(gunCam.fieldOfView, FOVinit, .1f);
			worldCam.fieldOfView = Mathf.Lerp(worldCam.fieldOfView, FOVinit, .1f);
		}

		Pick ();
	}

	[HideInInspector]
	public bool reloading = false;
	public void EndReloading()
	{
		reloading = false;
		animator.SetBool ("Reloading", false);

		int amtRefill = weapon.ClipSize - weapon.currentClip;

		if (weapon.ammoStockpile >= amtRefill)
		{
			weapon.currentClip += amtRefill;
			weapon.ammoStockpile -= amtRefill;
		}
		else
		{
			weapon.currentClip = weapon.ammoStockpile;
			weapon.ammoStockpile = 0;
		}
	}
	
	void Pick()
	{
		Ray ray = new Ray (this.transform.parent.position, this.transform.parent.forward);
		RaycastHit rh;
		bool isHit = Physics.Raycast (ray, out rh, 5.5f);

		interaction.text = "";
		if(isHit)
		{
			Pickable ri = rh.collider.gameObject.GetComponent<Pickable>();
			if(ri != null)
			{
				if(ri.GetComponent<AmmoBox>() != null)
					ri.GetComponent<AmmoBox>().SetHowMuch(weapon.maxAmmo - weapon.ammoStockpile, this);

				ri.OnHover.Invoke();
				interaction.text = ri.pickText;

				if(Input.GetKey(KeyCode.E))
				{
					ri.OnPick.Invoke();
				}
			}
		}
	}

	public void StartReloading()
	{
		reloading = true;
		animator.SetBool ("Reloading", true);
	}

	public void PlayClipOut()
	{
		myAudioSource.clip = weapon.ClipOut;
		myAudioSource.Play ();
	}

	public void PlayClipIn()
	{
		myAudioSource.clip = weapon.ClipIn;
		myAudioSource.Play ();
	}

	void FireBullet ()
	{
		GameObject muzF = (GameObject)GameObject.Instantiate (weapon.MuzzlePrefab, weapon.MuzzleTransform.position, weapon.MuzzleTransform.rotation);
		GameObject.Destroy (muzF, .2f);
		muzF.transform.SetParent (weapon.MuzzleTransform);
		weapon.FireRate = weapon.FireRateReset;
		this.transform.position -= this.transform.forward * weapon.RecoilTranslation;
		this.transform.Rotate (new Vector3 (-1, 0, 0) * weapon.RecoilRotation);
		Vector3 direction = this.transform.parent.transform.forward;
		direction.Normalize ();
		GameObject bul = (GameObject)GameObject.Instantiate (weapon.BulletPrefab, weapon.MuzzleTransform.position, weapon.MuzzleTransform.rotation);
		bul.GetComponent<Bullet> ().Set (weapon.MuzzleTransform.position, weapon.MuzzleTransform.position + direction * 3f, direction * 1.6f);
		myAudioSource.clip = weapon.FireNoise;
		myAudioSource.Play ();

		weapon.currentClip--;
		//raycasting
		Ray ray = new Ray (this.transform.parent.position, this.transform.parent.forward);
		RaycastHit rh;
		bool isHit = Physics.Raycast (ray, out rh, 100000);

		if(isHit)
		{
			RaycastInfo ri = rh.collider.gameObject.GetComponent<RaycastInfo>();
			BodyPart bp = rh.collider.gameObject.GetComponent<BodyPart>();
			if(ri != null)
			{
				Vector3 point = rh.point;
				Quaternion rot = Quaternion.LookRotation(rh.normal);
				GameObject ps = (GameObject)Instantiate(ri.onHitParticleSystem, point, rot);
			}
			else if(bp != null)
			{
				bp.DoDamage(rh.point, Quaternion.LookRotation(rh.normal), weapon.Damage, rh.collider.gameObject.name);
			}
		}
	}

	void FixedUpdate()
	{
		if(isFiring)
		{

		}
	}
	
}