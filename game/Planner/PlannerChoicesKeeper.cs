using System;
using Godot;

namespace TheLastGate.Planner;

// The planner's saved choices for whoever is logged in. A character made this session gets its number after the
// planner may already have looked, so the choices are reloaded whenever the number changes.
public sealed class PlannerChoicesKeeper(Func<uint> characterNumber)
{
    private static readonly string FilePath = ProjectSettings.GlobalizePath("user://" + PlannerChoicesStore.FileName);

    private PlannerChoices? choices;
    private uint loadedFor;

    public PlannerChoices Choices
    {
        get
        {
            var character = characterNumber();
            if (choices == null || loadedFor != character)
            {
                choices = PlannerChoicesStore.Load(FilePath, character);
                loadedFor = character;
            }

            return choices;
        }
    }

    public void Save()
    {
        if (loadedFor != 0 && !PlannerChoicesStore.Save(FilePath, loadedFor, Choices))
        {
            GD.PushWarning($"Couldn't save the skill planner's choices to {FilePath}");
        }
    }
}
