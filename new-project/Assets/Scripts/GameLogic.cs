using UnityEngine;
using UnityEngine.InputSystem;

public class GameLogic : MonoBehaviour
{
    public int count = 0;
    private bool clearShown = false;
    private float timeLeft = 30f; // 30-second limit
    private bool timeUp = false;
    private System.Collections.Generic.HashSet<string> collected = new System.Collections.Generic.HashSet<string>();
    private System.Collections.Generic.Dictionary<string, Vector3> initialPositions = new System.Collections.Generic.Dictionary<string, Vector3>();
    private System.Collections.Generic.Dictionary<string, GameObject> itemRefs = new System.Collections.Generic.Dictionary<string, GameObject>();

    // VFX state (public counters so the effects are verifiable via the bridge)
    public int vfxSpawned = 0;
    private Transform cam;
    private Vector3 camBasePos;
    private Quaternion camBaseRot;
    private float shakeUntil = 0f;
    private float shakeMag = 0f;
    private float countPulseUntil = 0f;
    private float clearPulseUntil = 0f;

    void Awake()
    {
        initialPositions["Player"] = transform.position;
        for (int i = 1; i <= 5; i++)
        {
            var item = GameObject.Find("Item_" + i);
            if (item != null)
            {
                itemRefs[item.name] = item; // keep references: Find() cannot see inactive items
                initialPositions[item.name] = item.transform.position;
            }
        }
        cam = Camera.main == null ? null : Camera.main.transform;
        if (cam != null) { camBasePos = cam.position; camBaseRot = cam.rotation; }
    }

    private bool rWasDown = false;

    void FixedUpdate()
    {
        if (!timeUp)
        {
            timeLeft -= Time.fixedDeltaTime;
            if (timeLeft <= 0f) { timeLeft = 0f; timeUp = true; } // game over: counting stops
        }

        var kb = Keyboard.current;
        if (kb == null) return;
        bool down = kb.rKey.isPressed; // edge-detect: works with state-write injection too
        if (down && !rWasDown) ResetAll(); // B5: R reset
        rWasDown = down;

        // Auto-return: fell off the floor edge / below the floor -> respawn at initial position.
        // count/collected/CLEAR are intentionally untouched.
        if (transform.position.y < -1f)
        {
            var rb = GetComponent<Rigidbody>();
            transform.position = initialPositions["Player"];
            if (rb != null) rb.linearVelocity = Vector3.zero;
        }
    }

    void LateUpdate()
    {
        if (cam == null) return;
        if (Time.time < shakeUntil)
        {
            cam.position = camBasePos + Random.insideUnitSphere * shakeMag;
            cam.rotation = camBaseRot * Quaternion.Euler(Random.insideUnitSphere * shakeMag * 10f);
        }
        else
        {
            cam.position = camBasePos;   // fixed camera: fully restored after each effect
            cam.rotation = camBaseRot;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (timeUp) return; // counting stops after the 30s limit
        if (!other.name.StartsWith("Item_")) return;
        if (collected.Contains(other.name)) return; // B3: no double counting
        collected.Add(other.name);
        count++;
        other.gameObject.SetActive(false); // item disappears
        SpawnBurst(other.transform.position, new Color(1f, 0.85f, 0.2f), 20); // pickup spark burst
        Shake(0.15f, 0.15f);
        countPulseUntil = Time.time + 0.3f;
        if (count >= 5)
        {
            clearShown = true; // B4: CLEAR only when all 5
            clearPulseUntil = Time.time + 1.2f;
            StartCoroutine(Celebrate());
        }
    }

    System.Collections.IEnumerator Celebrate()
    {
        Color[] colors = { new Color(1f, 0.3f, 0.3f), new Color(1f, 0.7f, 0.2f), new Color(0.3f, 1f, 0.4f), new Color(0.2f, 0.8f, 1f), new Color(1f, 0.4f, 0.9f) };
        for (int i = 0; i < 5; i++)
        {
            SpawnBurst(transform.position + Vector3.up * 1.5f, colors[i], 30);
            Shake(0.2f, 0.3f);
            yield return new WaitForSeconds(0.15f);
        }
    }

    void SpawnBurst(Vector3 pos, Color color, int particles)
    {
        var go = new GameObject("VFX_Burst");
        go.transform.position = pos;
        var ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 1f; main.loop = false; main.playOnAwake = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.9f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(4f, 8f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.18f);
        main.startColor = color;
        main.maxParticles = particles;
        var em = ps.emission; em.rateOverTime = 0f;
        var sh = ps.shape; sh.enabled = true; sh.shapeType = ParticleSystemShapeType.SphereShell; sh.radius = 0.05f;
        var rend = go.GetComponent<ParticleSystemRenderer>();
        if (rend == null) rend = go.AddComponent<ParticleSystemRenderer>();
        Shader s = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (s == null) s = Shader.Find("Particles/Standard Unlit");
        if (s == null) s = Shader.Find("Sprites/Default");
        if (s != null) rend.material = new Material(s);
        vfxSpawned++;
        ps.Play();
        ps.Emit(particles); // one-shot burst
        Destroy(go, 1.5f);
    }

    void Shake(float seconds, float mag) { shakeUntil = Time.time + seconds; shakeMag = Mathf.Max(shakeMag, mag); }

    void ResetAll()
    {
        transform.position = initialPositions["Player"];
        var rb = GetComponent<Rigidbody>();
        if (rb != null) rb.linearVelocity = Vector3.zero; // B5: velocity too
        count = 0;
        clearShown = false;
        timeLeft = 30f; // B5: timer re-initialized on reset too
        timeUp = false;
        collected.Clear();
        vfxSpawned = 0;
        shakeUntil = 0f; shakeMag = 0f;
        countPulseUntil = 0f; clearPulseUntil = 0f;
        if (cam != null) { cam.position = camBasePos; cam.rotation = camBaseRot; } // fixed camera restored
        foreach (var kv in initialPositions)
        {
            if (kv.Key == "Player") continue;
            var item = itemRefs[kv.Key]; // stored references, not Find()
            if (item != null)
            {
                item.SetActive(true);
                item.transform.position = kv.Value;
            }
        }
    }

    void OnGUI()
    {
        float cs = Time.time < countPulseUntil ? 1.5f : 1f;
        GUI.matrix = Matrix4x4.TRS(new Vector3(20, 20, 0), Quaternion.identity, new Vector3(cs, cs, 1f));
        GUI.Label(new Rect(0, 0, 400, 30), "Collect: " + count + " / 5"); // B1: shows 0 at start
        GUI.matrix = Matrix4x4.TRS(new Vector3(20, 60, 0), Quaternion.identity, Vector3.one);
        GUI.Label(new Rect(0, 0, 400, 30), "Time: " + Mathf.CeilToInt(timeLeft)); // 30s limit countdown
        if (timeUp) GUI.Label(new Rect(0, 40, 400, 40), "TIME UP");
        if (clearShown)
        {
            float s = Time.time < clearPulseUntil ? 2.2f : 1.5f;
            GUI.matrix = Matrix4x4.TRS(new Vector3(20, 100, 0), Quaternion.identity, new Vector3(s, s, 1f));
            GUI.Label(new Rect(0, 0, 400, 40), "CLEAR!");
        }
    }
}
