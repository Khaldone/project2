// Assets/_Project/Scenes/UI_Leaderboard/Scripts/LeaderboardView.cs
using System;
using UnityEngine;


public interface ILeaderboardView
{
    // The Presenter tells the View how many total items exist
    void SetTotalItemCount(int count);

    // The View asks the Presenter for data when a row recycles
    event Func<int, LeaguePlayer> OnRequestRowData;
}

public class LeaderboardView : MonoBehaviour, ILeaderboardView
{
    //[SerializeField] private RecycledScrollView _scrollView; // A generic reusable UI component


    public event Func<int, LeaguePlayer> OnRequestRowData;


    private void Awake()
    {
        // When the ScrollView recycles a UI GameObject, it triggers this callback
        // passing the UI component and the new index it represents (e.g., Row 45)
        //_scrollView.OnRowRecycled += HandleRowRecycled;
    }


    public void SetTotalItemCount(int count)
    {
        // Tells the scroll view to adjust its scrollbar size to simulate 1,000 items,
        // even though it's only using 12 GameObjects.
        //_scrollView.Initialize(count);
    }


    private void HandleRowRecycled(GameObject rowObject, int dataIndex)
    {
        // 1. Ask the Presenter for the pure C# data at this index
        LeaguePlayer data = OnRequestRowData?.Invoke(dataIndex) ?? default;


        // 2. Grab the dumb UI script on the row and paint the new data
        //var rowUI = rowObject.GetComponent<PlayerRowUI>();
        //rowUI.SetData(data.Rank, data.DisplayName, data.Trophies);
    }
}
