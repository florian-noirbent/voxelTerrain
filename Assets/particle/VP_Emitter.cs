using UnityEngine;
using System.Collections;
	
public class VP_Emitter : MonoBehaviour {
	
	public ParticleSystem.Particle[] particles;
	private int[] deadParticles;
	
	public int chunkSize = 512;
	private int liveParticleCount = 0;
	private Vector3 lastSpawnPosition = Vector3.zero;
	private float lastSpawnTime = 0f;
	
	private int _particleCount = 1024;
	private double debugTime = 0;
	
	void Start(){
		_particleCount = chunkSize*2;
		particles = new ParticleSystem.Particle[_particleCount];
		deadParticles = new int[_particleCount];
		Clear();
	}
	
	public void Clear(bool reset = false){
		int i;
		if(reset) Resize (chunkSize*2);
		for(i=0; i<_particleCount; i++){
			deadParticles[i] = i;
			particles[i].startLifetime = 0f;
		}
		liveParticleCount = 0;
	}
	
	// redimensionnement de la liste de particules
	public void Resize(int newParticleCount){		
		_particleCount = newParticleCount;
		ParticleSystem.Particle[] tmp_particles = new ParticleSystem.Particle[_particleCount];
		int[] tmp_deadParticles = new int[_particleCount];

		int count = particles.Length;
		int i=0;
		int j=0;
		for(i=0;i<count;i++){
			if(particles[i].startLifetime > 0){
				tmp_particles[j] = particles[i];				
				tmp_deadParticles[j] = -1;
				j++;
			}
		}
		for(i=j;i<_particleCount;i++){
			tmp_deadParticles[i] = i;
		}
		
		particles = tmp_particles;
		deadParticles = tmp_deadParticles;
	}
	
	// création d'une particules
	public int SpawnParticle(Vector3 position, float startLifetime = 1f, float size = 1f, Color color = default(Color), Vector3 velocity = default(Vector3)){
		
		if(liveParticleCount >= _particleCount) Resize (_particleCount + chunkSize);
		
		if(liveParticleCount < _particleCount){
			int index = deadParticles[liveParticleCount];
			deadParticles[liveParticleCount] = -1;
			
			liveParticleCount++;
			
			particles[index].position = position;
			particles[index].startLifetime = startLifetime;
			particles[index].lifetime = 0f;
			particles[index].size = size;
			particles[index].color = color;
			particles[index].velocity = velocity;
			
			return index;
		}
		
		return -1;
	}
	
	// trace une ligne de particules
	public void SpawnRow(Vector3 endPosition, float stepDistance = 1f, float startLifetime = 1f, float size = 1f, Color color = default(Color), Vector3 velocity = default(Vector3)){
		SpawnRow(lastSpawnPosition, endPosition, stepDistance, startLifetime ,  size, color, velocity);	
	}
	
	public void SpawnRow(Vector3 startPosition, Vector3 endPosition, float stepDistance = 1f, float startLifetime = 1f, float size = 1f, Color color = default(Color), Vector3 velocity = default(Vector3)){
		
		float elapsed = (Time.time - lastSpawnTime);
		bool linkRow = elapsed < 0.1;
		
		if(!linkRow) startPosition = endPosition;
		
		Vector3 dir = endPosition - startPosition;
		int count = Mathf.FloorToInt(dir.magnitude / stepDistance);
		
		lastSpawnPosition = endPosition;
		
		if(count < 2){
			SpawnParticle(endPosition, startLifetime, size , color, velocity);
		}else{

			int i;
			float stepDist = dir.magnitude / count;
			dir = dir.normalized * stepDist;
			int index;
			float age = 0;
			float stepTime = elapsed / count;
			for(i=0;i<count;i++){
				
				startLifetime = Random.Range(5f,15f);
				color = new Color(Random.Range(0f,1f),Random.Range(0f,1f),Random.Range(0f,1f));
				velocity = Random.onUnitSphere * 0.1f;
				
				index = SpawnParticle(endPosition, startLifetime, size , color, velocity);
				if(index > -1){
					particles[index].lifetime = age;
					particles[index].position += velocity * age;
				}
				age += stepTime;
				endPosition -= dir;
			}
		}
		
		lastSpawnTime = Time.time;
		
	}
	
	public void KillParticle(int index){
		if(index < 0 || index >= _particleCount) return;
		if(liveParticleCount > 0 && particles[index].startLifetime > 0){
			liveParticleCount--;
			deadParticles[liveParticleCount] = index;
			particles[index].startLifetime = 0f;
		}
	}
	
	void Update () {
		double now = Time.realtimeSinceStartup;
		int i;
		
		// création des particules à la souris
		if(Input.GetMouseButton(0)){
			Ray r = Camera.mainCamera.ScreenPointToRay (Input.mousePosition);
			Vector3 pos = r.origin + r.direction.normalized * Vector3.Distance(Vector3.zero,Camera.mainCamera.transform.position);
			int n = Random.Range (10,30);
			for(i=1;i<n;i++){
				SpawnRow(pos,0.1f,Random.Range(5f,15f),0.1f,new Color(Random.Range(0f,1f),Random.Range(0f,1f),Random.Range(0f,1f)),Random.onUnitSphere * 0.1f);
			}
		}

		// mise à jour des particules
		int updatedCount = 0;
		int particlesToUpdate = liveParticleCount;
		float t = Time.deltaTime;

		for(i=0; i<_particleCount; i++){
			if(particles[i].startLifetime > 0){
				updatedCount ++;
				particles[i].lifetime += t;
				if(particles[i].lifetime <= particles[i].startLifetime){
					particles[i].position += particles[i].velocity * t;
					
					// affichage de debug
					Debug.DrawLine(particles[i].position + Vector3.up * particles[i].size * 0.5f , particles[i].position + Vector3.down * particles[i].size * 0.5f, particles[i].color);
					Debug.DrawLine(particles[i].position + Vector3.left * particles[i].size * 0.5f , particles[i].position + Vector3.right * particles[i].size * 0.5f, particles[i].color);
					Debug.DrawLine(particles[i].position + Vector3.forward * particles[i].size * 0.5f , particles[i].position + Vector3.back * particles[i].size * 0.5f, particles[i].color);
					
				}else{
					KillParticle(i);
				}
				if(updatedCount >= particlesToUpdate) break;
			}
		}
		
		// s'il y a peu de particules utilisées, réduction du tableau général
		if(liveParticleCount < _particleCount * 0.5f && _particleCount > chunkSize && !Input.GetMouseButton(0)){
			Resize ((int)(_particleCount * 0.5f + chunkSize));
		}
		
		debugTime = Time.realtimeSinceStartup - now;
	}
	
	
	
	void OnGUI() {
		
		GUILayout.Space(20);
		GUILayout.Label(liveParticleCount.ToString()+"/"+_particleCount.ToString());
		
		GUI.color = Color.white;
		if(debugTime < 0.0001) debugTime = 0;
		GUILayout.Label(((float)debugTime).ToString()+"ms");
		
		bool dbg = Input.GetKey(KeyCode.Space);
		if(dbg){
			GUILayout.Space(20);
			int i = 0;
			Vector2 pos;
			for(i=0;i<_particleCount; i++){
				if(particles[i].startLifetime > 0){
					GUI.color = particles[i].color;
					pos = Camera.mainCamera.WorldToScreenPoint(particles[i].position);
					GUI.Label(new Rect(pos.x,Screen.height-pos.y,30,20),i.ToString());
				}
				
			}
		}
	}
	
}
