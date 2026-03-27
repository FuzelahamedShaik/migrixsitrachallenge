return await App.Run(args);

public record SessionIdentity(string UserId);
public record ClientParameters(string Name = "Ikon"); // Can be set through URL query parameter ?name=value

[App]
public class IkonDemoApp(IApp<SessionIdentity, ClientParameters> app)
{
    private UI UI { get; } = new(app, new Theme());
    private Audio Audio { get; } = new(app);

    private readonly Reactive<string> _name = new("");
    private readonly Reactive<string> _imagePrompt = new("A neon-lit cyberpunk street market at night with holographic signs");
    private readonly Reactive<byte[]?> _generatedImageData = new(null);
    private readonly Reactive<string?> _generatedImageMime = new(null);
    private readonly Reactive<bool> _isGeneratingImage = new(false);
    private readonly Reactive<string> _speechText = new("The quick brown fox jumps over the lazy dog, speaking every letter in style.");
    private readonly Reactive<bool> _isSpeaking = new(false);
    private readonly ClientReactive<string> _currentTheme = new(Constants.LightTheme);

    public async Task Main()
    {
        app.ClientJoinedAsync += async args =>
        {
            if (!string.IsNullOrEmpty(args.ClientContext.Theme))
            {
                _currentTheme.Value = args.ClientContext.Theme == Constants.DarkTheme ? Constants.DarkTheme : Constants.LightTheme;
            }
        };

        UI.Root([Page.Default],
            content: view =>
            {
                view.Column([Container.Xl2, Layout.Center, "py-8 px-4 min-h-screen"], content: view =>
                {
                    view.Column([Card.Default, Layout.Column.Lg, "p-10 w-full"], content: view =>
                    {
                        // Header with title and theme toggle
                        view.Row([Layout.Row.SpaceBetween, "mb-6"], content: view =>
                        {
                            view.Text([
                                    "text-4xl font-bold tracking-tight font-heading",
                                    "wave:motion-[0:translate-y-0,50:translate-y-[-10px],100:translate-y-0] wave:motion-duration-2500ms wave:motion-stagger-150ms wave:motion-per-letter wave:motion-loop wave:motion-ease-[ease-in-out]"
                                ], $"{nameof(IkonDemoApp)}");

                            view.Button([Button.GhostMd, Button.Size.Icon],
                                onClick: ToggleThemeAsync,
                                content: v => v.Icon([Icon.Default], name: _currentTheme.Value == Constants.DarkTheme ? "sun" : "moon"));
                        });

                        // Greeting section
                        view.Column([Layout.Column.Sm], content: view =>
                        {
                            view.Text([Text.H2], string.IsNullOrWhiteSpace(_name.Value) ? nameof(IkonDemoApp) : $"Hello, {_name.Value}!");

                            view.Column([FormField.Root], content: view =>
                            {
                                view.TextField(
                                    [Input.Default],
                                    placeholder: "Enter your name",
                                    value: _name.Value,
                                    onValueChange: async value => { _name.Value = value; }
                                );
                            });
                        });

                        // Image generation section
                        view.Column([Card.Default, "p-4"], content: view =>
                        {
                            view.Column([FormField.Root], content: view =>
                            {
                                view.Text([FormField.Label], "Image Prompt");
                                view.TextField(
                                    [Input.Default],
                                    placeholder: "Describe the image you want to generate",
                                    value: _imagePrompt.Value,
                                    onValueChange: async value => { _imagePrompt.Value = value; }
                                );
                            });

                            view.Button(
                                [Button.PrimaryMd, "mt-2 w-fit self-center"],
                                _isGeneratingImage.Value ? "Generating..." : "Create image",
                                disabled: _isGeneratingImage.Value || string.IsNullOrWhiteSpace(_imagePrompt.Value),
                                onClick: async () => { await GenerateImageAsync(); }
                            );

                            if (_generatedImageData.Value != null && _generatedImageMime.Value != null)
                            {
                                view.Box(["mt-4"], content: view =>
                                {
                                    view.Image(style: ["max-w-full h-auto", Tokens.Radius.Lg], data: _generatedImageData.Value, mimeType: _generatedImageMime.Value, alt: "Generated image");
                                });
                            }
                        });

                        // TTS section
                        view.Column([Card.Default, "p-4"], content: view =>
                        {
                            view.Column([FormField.Root], content: view =>
                            {
                                view.Text([FormField.Label], "Text to Speech");
                                view.TextField(
                                    [Input.Default],
                                    placeholder: "Enter text to speak",
                                    value: _speechText.Value,
                                    onValueChange: async value => { _speechText.Value = value; }
                                );
                            });

                            view.Button(
                                [Button.PrimaryMd, "mt-2 w-fit self-center"],
                                _isSpeaking.Value ? "Speaking..." : "Speak",
                                disabled: _isSpeaking.Value || string.IsNullOrWhiteSpace(_speechText.Value),
                                onClick: async () => { await SpeakAsync(); }
                            );
                        });
                    });
                });
            });
    }

    private async Task ToggleThemeAsync()
    {
        var nextTheme = _currentTheme.Value == Constants.DarkTheme ? Constants.LightTheme : Constants.DarkTheme;
        var updated = await ClientFunctions.SetThemeAsync(nextTheme);

        if (updated)
        {
            _currentTheme.Value = nextTheme;
        }
    }

    private async Task GenerateImageAsync()
    {
        if (_isGeneratingImage.Value || string.IsNullOrWhiteSpace(_imagePrompt.Value))
        {
            return;
        }

        _isGeneratingImage.Value = true;

        try
        {
            var imageGenerator = new ImageGenerator(ImageGeneratorModel.Gemini25FlashImage);

            var results = await imageGenerator.GenerateImageAsync(new ImageGeneratorConfig
            {
                Prompt = _imagePrompt.Value,
                Width = 512,
                Height = 512
            });

            if (results.Count > 0)
            {
                var result = results[0];
                _generatedImageData.Value = result.Data;
                _generatedImageMime.Value = result.MimeType;
            }
        }
        catch (Exception ex)
        {
            Log.Instance.Warning($"Image generation failed: {ex.Message}");
        }
        finally
        {
            _isGeneratingImage.Value = false;
        }
    }

    private async Task SpeakAsync()
    {
        if (_isSpeaking.Value || string.IsNullOrWhiteSpace(_speechText.Value))
        {
            return;
        }

        _isSpeaking.Value = true;

        try
        {
            using var speechGenerator = new SpeechGenerator(SpeechGeneratorModel.ElevenTurbo25);

            await foreach (var audio in speechGenerator.GenerateSpeechAsync(new SpeechGeneratorConfig { Text = _speechText.Value }))
            {
                Audio.SendSpeech(audio);
            }
        }
        catch (Exception ex)
        {
            Log.Instance.Warning($"Speech generation failed: {ex.Message}");
        }
        finally
        {
            _isSpeaking.Value = false;
        }
    }
}

// Minimal pipeline example that appends "Hello " to the content of each input item
[Pipeline(name: "example")]
public class ExamplePipeline
{
    public async Task Run(Pipeline<Item>.Branch inputItems, CancellationToken cancellationToken)
    {
        var outputItems = inputItems.Transform(item => Process(item, cancellationToken));
        outputItems.Output();
    }

    [Processor]
    private static async Task<List<Item>> Process(Item inputItem, CancellationToken cancellationToken)
    {
        var text = await inputItem.GetContentAsString();
        var outputItem = await Item.Create(inputItem, $"{inputItem.Name}.full_greeting", $"Hello {text}!", MimeTypes.TextPlain);
        return [outputItem];
    }
}
