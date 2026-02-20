using Godot;
using VikingJamGame.Models.Navigation;

namespace VikingJamGame.GameLogic.Nodes;

[GlobalClass]
public partial class GodotMapNode : Node2D
{
    private const string UNKNOWN_LABEL_TEXT = "Unknown";

    [Export] private Sprite2D Sprite { get; set; } = null!;
    [Export] private Label Label { get; set; } = null!;
    [Export] private Texture2D? UnknownTexture { get; set; }

    public NavigationMapNode MapNode { get; private set; } = null!;

    private string _knownLabelText = string.Empty;
    private Texture2D? _knownTexture;
    private bool _hasKnownIdentity;

    public void Initialize(NavigationMapNode mapNode, Texture2D? texture)
    {
        MapNode = mapNode;
        Sprite.Texture = texture;
        Label.Text = mapNode.Kind;
        _knownTexture = Sprite.Texture;
        _knownLabelText = Label.Text;
        _hasKnownIdentity = true;
        SetIdentityKnown(false);
    }

    public void SetIdentityKnown(bool isKnown)
    {
        if (!_hasKnownIdentity)
        {
            CaptureCurrentIdentity();
        }

        if (isKnown)
        {
            Sprite.Texture = _knownTexture;
            Label.Text = _knownLabelText;
            return;
        }

        Sprite.Texture = UnknownTexture ?? Sprite.Texture;
        Label.Text = UNKNOWN_LABEL_TEXT;
    }

    private void CaptureCurrentIdentity()
    {
        _knownTexture = Sprite.Texture;
        _knownLabelText = Label.Text;
        _hasKnownIdentity = true;
    }
}
