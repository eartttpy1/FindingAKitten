using UnityEngine;

[System.Serializable]
public class RandomAudio
{
    public AudioClip clip;
    [Range(0, 100)] public float weight = 1f; // โอกาสออก (ยิ่งเลขเยอะ ยิ่งออกบ่อย)
}

public class HealthPickup : MonoBehaviour
{
    [Header("Settings")]
    public float healAmount = 25f;

    [Header("Random Effects")]
    public RandomAudio[] pickupSounds; // อาเรย์ของเสียงที่สุ่มได้
    public GameObject pickupEffect;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                if (playerHealth.currentHealth < playerHealth.maxHealth)
                {
                    playerHealth.Heal(healAmount);
                    
                    // --- ส่วนของการเล่นเสียงแบบสุ่ม ---
                    PlayRandomPickupSound();

                    if (pickupEffect != null)
                    {
                        Instantiate(pickupEffect, transform.position, Quaternion.identity);
                    }

                    Destroy(gameObject);
                }
            }
        }
    }

    private void PlayRandomPickupSound()
    {
        if (pickupSounds == null || pickupSounds.Length == 0) return;

        // 1. คำนวณผลรวมของ Weight ทั้งหมด
        float totalWeight = 0f;
        foreach (var sound in pickupSounds)
        {
            totalWeight += sound.weight;
        }

        // 2. สุ่มตัวเลขตั้งแต่ 0 ถึง totalWeight
        float randomValue = Random.Range(0f, totalWeight);

        // 3. วนลูปเช็คว่าเลขที่สุ่มได้ ตกอยู่ในช่วงของเสียงไหน
        float currentWeightSum = 0f;
        foreach (var sound in pickupSounds)
        {
            currentWeightSum += sound.weight;
            if (randomValue <= currentWeightSum)
            {
                // เล่นเสียงที่เลือกได้
                SpawnAudioObject(sound.clip);
                break; // เจอแล้วหยุดลูป
            }
        }
    }

    private void SpawnAudioObject(AudioClip clip)
    {
        if (clip == null) return;

        GameObject audioObj = new GameObject("Random Pickup Sound: " + clip.name);
        AudioSource audioSrc = audioObj.AddComponent<AudioSource>();
        audioSrc.clip = clip;
        audioSrc.spatialBlend = 0f; // 2D Sound
        audioSrc.Play();

        Destroy(audioObj, clip.length);
    }
}