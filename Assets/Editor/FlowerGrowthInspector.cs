using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(player_move))]
public class FlowerGrowthInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (!Application.isPlaying)
            return;

        var player = (player_move)target;
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Runtime growth diagnostics", EditorStyles.boldLabel);
        using (new EditorGUI.DisabledScope(true))
        {
            EditorGUILayout.EnumPopup("State", player.State);
            EditorGUILayout.EnumPopup("Waiting reason", player.WaitingReason);
            EditorGUILayout.Vector2Field("Direction", player.GrowthDirection);
            EditorGUILayout.Toggle("Restart suggested", player.RestartSuggested);
        }
        if (player.State == player_move.GrowthState.Waiting)
        {
            string message = player.WaitingReason == player_move.StopCause.TipCovered
                ? "Верхушка перекрыта блоком. Уберите блок, чтобы продолжить рост."
                : player.WaitingReason == player_move.StopCause.NoForwardRoute
                    ? "Нет допустимого продолжения вперёд. Цветок ждёт изменения препятствий."
                    : "Следующий шаг заблокирован геометрией препятствия.";
            EditorGUILayout.HelpBox(message, MessageType.Info);
        }
        if (player.enabled)
            Repaint();
    }
}
