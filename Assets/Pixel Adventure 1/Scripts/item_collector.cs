using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class item_collector : MonoBehaviour
{
    // 库存面板
    public GameObject inventoryPanel;
    // 物品丢弃位置
    public Transform dropPosition;
    // UI物品槽数组
    public ItemSlotUI[] uiSlots;
    // 物品槽数组
    public ItemSlot[] slots;

    [Header("选中物品")]
    private ItemSlot selectedItem;
    // 当前选中的物品槽索引
    private int selectedItemIndex;
    // 选中物品的名称、描述和属性显示
    public TextMeshProUGUI selectedItemName, selectedItemDescription, selectedItemstatName, selectedItemsStatValue;
    // 各种按钮：使用、装备、取消装备、丢弃
    public GameObject useButton, equipButton, unequipButton, dropButton;

    // 当前装备的物品索引
    private int currentequipIndex;

    // 创建一个静态实例，便于全局访问
    public static item_collector instance;

    [SerializeField] private AudioSource collectSoundEffect;

    // 在Awake函数中初始化实例
    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // 游戏开始时关闭库存面板
        inventoryPanel.SetActive(false);

        // 初始化物品槽，因为没有直接附加脚本到物品槽
        slots = new ItemSlot[uiSlots.Length];

        // 初始化每个物品槽
        for (int x = 0; x < slots.Length; x++)
        {
            slots[x] = new ItemSlot();  // 为每个物品槽分配新的对象
            uiSlots[x].index = x;   // 设置物品槽索引
            uiSlots[x].Clear(); // 清空物品槽，表示未持有任何物品
        }

        // 清除选中的物品窗口
        ClearSelectItemWindow();
    }

    void Update()
    {
        // 按下'I'键打开/关闭库存
        if (Input.GetKeyDown(KeyCode.I))
        {
            OpenInventory();
        }
    }

    // 添加物品到库存
    public void AddItem(ItemData item)
    {
        // 如果物品可以堆叠
        if (item.canStack)
        {
            // 获取可以堆叠的物品槽
            ItemSlot slotToStackTo = GetItemStack(item);

            if (slotToStackTo != null)
            {
                // 增加物品数量并更新UI
                slotToStackTo.quantity++;
                UpdateUI();
                collectSoundEffect.Play();
                return;
            }
        }

        // 获取空的物品槽
        ItemSlot emptySlot = GetEmptySlot();

        if (emptySlot != null)
        {
            // 如果有空槽，添加物品
            emptySlot.item = item;
            emptySlot.quantity = 1;

            UpdateUI();
            collectSoundEffect.Play();
            return;
        }

        // 如果没有空槽，则丢弃物品
        ThrowItem(item);
    }

    // 丢弃物品
    public void ThrowItem(ItemData item)
    {
        // 在丢弃位置生成物品
        Instantiate(item.dropPrefab, dropPosition.position, dropPosition.rotation);
    }

    // 更新UI以显示库存中的物品
    void UpdateUI()
    {
        // 遍历所有物品槽
        for (int x = 0; x < slots.Length; x++)
        {
            // 如果槽内有物品
            if (slots[x].item != null)
            {
                // 显示该物品
                uiSlots[x].Set(slots[x]);
            }
            else
            {
                // 清空物品槽
                uiSlots[x].Clear();
            }
        }
    }

    // 获取可以堆叠的物品槽
    ItemSlot GetItemStack(ItemData item)
    {
        for (int x = 0; x < slots.Length; x++)
        {
            // 检查物品是否可以继续堆叠
            if (slots[x].item == item && slots[x].quantity < item.maxStackAmount)
            {
                // 返回可以堆叠的物品槽
                return slots[x];
            }
        }
        // 如果没有可以堆叠的物品槽，则返回null
        return null;
    }

    // 获取空的物品槽
    ItemSlot GetEmptySlot()
    {
        for (int x = 0; x < slots.Length; x++)
        {
            // 如果槽是空的
            if (slots[x].item == null)
            {
                // 返回空的物品槽
                return slots[x];
            }
        }
        // 如果没有空槽，则返回null
        return null;
    }

    // 选择物品
    public void SelectItem(int index)
    {
        Debug.Log("Selected Item Index: " + index + ", Item: " + slots[index].item);
        // 如果选择的槽为空，直接返回
        if (slots[index].item == null)
        {
            return;
        }

        // 设置选中的物品和索引
        selectedItem = slots[index];
        selectedItemIndex = index;

        // 显示物品的名称和描述
        selectedItemName.text = selectedItem.item.displayName;
        selectedItemDescription.text = selectedItem.item.description;

        selectedItemstatName.text = string.Empty;
        selectedItemsStatValue.text = string.Empty;

        

        for (int x = 0; x < selectedItem.item.consumables.Length; x++) {
            selectedItemstatName.text += selectedItem.item.consumables[x].type.ToString() + "\n";
            selectedItemsStatValue.text += selectedItem.item.consumables[x].value.ToString() + "\n";
        }

        // 根据物品类型显示相应的按钮
        useButton.SetActive(selectedItem.item.type == ItemType.Consumable);
        equipButton.SetActive(selectedItem.item.type == ItemType.Equipable && !uiSlots[index].equipped);
        unequipButton.SetActive(selectedItem.item.type == ItemType.Equipable && uiSlots[index].equipped);
        dropButton.SetActive(true);
    }

    // 清除选中的物品窗口
    void ClearSelectItemWindow()
    {
        selectedItem = null;
        selectedItemName.text = string.Empty;
        selectedItemDescription.text = string.Empty;
        selectedItemsStatValue.text = string.Empty;
        selectedItemstatName.text = string.Empty;
        useButton.SetActive(false);
        equipButton.SetActive(false);
        unequipButton.SetActive(false);
        dropButton.SetActive(false);
    }

    // 使用物品按钮点击事件
    public void ONUseButton()
    {
        if(selectedItem.item.type == ItemType.Consumable)
        {
            for(int x = 0;x < selectedItem.item.consumables.Length; x++)
            {
                switch (selectedItem.item.consumables[x].type)
                {
                    case ConsumableType.Health: PlayerLife.instance.Heal(selectedItem.item.consumables[x].value);break;
                   // case ConsumableType.Magic: PlayerLife.instance.Heal(selectedItem.item.consumables[x].value); break;
                }
            }
        }
        RemoveSelectedItem();
    }

    // 装备物品按钮点击事件
    public void OnEquipButton()
    {
        if (uiSlots[currentequipIndex].equipped)
        {
            UnEquip(currentequipIndex);
        }

        uiSlots[selectedItemIndex].equipped = true;
        currentequipIndex = selectedItemIndex;
        EquipManager.instance.EquipNew(selectedItem.item);
        UpdateUI();
        SelectItem(selectedItemIndex);
    }

    // 取消装备物品
    void UnEquip(int index)
    {
        uiSlots[index].equipped = false;
        EquipManager.instance.Unequip();
        UpdateUI();

        if (selectedItemIndex == index)
        {
            SelectItem(index);
        }

    }

    // 取消装备按钮点击事件
    public void OnUnequipButton()
    {
        UnEquip(selectedItemIndex);
    }

    // 丢弃物品按钮点击事件
    public void OnDropButton()
    {
        // 检查选中的物品是否为空
        if (selectedItem != null && selectedItem.item != null)
        {
            // 丢弃选中的物品
            ThrowItem(selectedItem.item);
            // 移除选中的物品
            RemoveSelectedItem();
        }
        else
        {
            Debug.LogWarning("No item selected to drop!");
        }
    }

    // 移除选中的物品
    void RemoveSelectedItem()
    {
        selectedItem.quantity -= 1;

        // 如果物品数量为0
        if (selectedItem.quantity == 0)
        {
            // 如果物品已经被装备，取消装备
            if (uiSlots[selectedItemIndex].equipped == true)
            {
                UnEquip(selectedItemIndex);
            }
            selectedItem.item = null;
            // 清空物品窗口
            ClearSelectItemWindow();
        }
        // 更新UI
        UpdateUI();
    }

    // 移除指定物品
    public void RemoveItem(ItemData item)
    {

    }

    // 检查是否拥有指定数量的物品
    public bool HasItem(ItemData item, int quantity)
    {
        return false;
    }

    // 打开库存
    public void OpenInventory()
    {
        inventoryPanel.SetActive(true);

        Time.timeScale = 0;        // 暂停游戏时间
        ClearSelectItemWindow();
    }

    // 关闭库存
    public void CloseInventory()
    {
        inventoryPanel.SetActive(false);

        Time.timeScale = 1;        // 恢复游戏时间
    }
}

// 定义物品槽类
public class ItemSlot
{
    // 物品数据
    public ItemData item;
    // 物品数量
    public int quantity;
}
