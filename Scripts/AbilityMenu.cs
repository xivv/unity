using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AbilityMenu : MonoBehaviour
{

    public List<Ability> abilities = new List<Ability>();

    private Text[] abilityTextFields;
    public Ability selectedAbility;

    private bool isMoving = false;
    [HideInInspector]
    public bool canAct = false;

    public float movementDelay = 0.2f;
    private int index = 0;
    private int maxItems = 8;


    public void setAbilities(List<Ability> list)
    {
        this.abilities = list;
        for (int i = 0; i < this.abilities.Count; i++)
        {
            this.abilityTextFields[i].text = this.abilities[i].name;
        }
        this.maxItems = this.abilities.Count;
        for (int i = this.maxItems; i < this.abilityTextFields.Length; i++)
        {
            this.abilityTextFields[i].gameObject.SetActive(false);
        }
        SelectAbility();
    }
    void SelectAbility()
    {
        this.abilityTextFields[index].GetComponentInParent<Image>().color = Color.green;
    }

    void DeselectAbility()
    {
        this.abilityTextFields[index].GetComponentInParent<Image>().color = Color.red;
    }

    // Start is called before the first frame update
    void Awake()
    {
        abilityTextFields = this.GetComponentsInChildren<Text>();
    }

    // Update is called once per frame
    void Update()
    {

        if (isMoving || !canAct) return;

        int vertical = 0;
        vertical = (int)(Input.GetAxisRaw("Vertical")) * -1;

        if (vertical != 0)
        {
            this.isMoving = true;
            DeselectAbility();

            index += vertical;

            if (index >= maxItems)
            {
                index = 0;
            }
            else if (index < 0)
            {
                index = this.maxItems - 1;
            }

            SelectAbility();
            Invoke("resetMovement", movementDelay);
        }
    }

    void OnGUI()
    {

        if (canAct)
        {
            if (Event.current.Equals(Event.KeyboardEvent(KeyCode.KeypadEnter.ToString())) || Event.current.Equals(Event.KeyboardEvent(KeyCode.Return.ToString())))
            {
                // We choose the ability and need to now select targets
                this.selectedAbility = this.abilities[index];
            }

            if (Event.current.Equals(Event.KeyboardEvent(KeyCode.Escape.ToString())))
            {
                // We cancel
                this.canAct = false;
            }
        }

    }

    protected void resetMovement()
    {
        this.isMoving = false;
    }
}
