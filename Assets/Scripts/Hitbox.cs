using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This is the attack box
public class Hitbox : MonoBehaviour
{
    public CharacterObject character;
    [IndexedItem(IndexedItemAttribute.IndexedItemType.STATES)]
    public int stateIndex;

    // Start is called before the first frame update
    void Start()
    {
        character = transform.root.GetComponent<CharacterObject>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject != transform.root.gameObject)
        {
            // this is really slow
            if (character.hitActive > 0)
            {
                CharacterObject victim = other.transform.root.GetComponent<CharacterObject>();
                victim.GetHit(character);
            }
            
            //Debug.Log("HIT!");
        }
    }
}
