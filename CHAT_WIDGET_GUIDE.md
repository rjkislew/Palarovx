# Chat Widget Implementation Guide

## Overview
A fully functional, minimalist chat widget has been implemented in the Palaro 2026 Blazor WebAssembly application. The widget appears as a floating chat button in the bottom-right corner of every page with a clean, professional design.

## Current Features

### ?? **Design**
- **Minimalist UI**: Clean, modern design with navy and gold accents
- **Color Palette**:
  - Primary: `#0f172a` (Deep Navy)
  - Accent: `#f59e0b` (Sport Gold)
  - Background: `#ffffff` (White)
  - Gray: `#f8fafc` (Light Gray)

### ? **Functionality**
- **Floating Chat Button**: Always visible in bottom-right (customizable position)
- **Expandable Chat Window**: Smooth slide-in animation from bottom-right
- **Auto-Expanding Textarea**: Grows as user types (max height 120px)
- **Rich Message Formatting**: Markdown-like syntax support
- **AI Avatar Display**: Shows Palaro logo in messages
- **Real-time Updates**: Messages update instantly
- **Typing Indicator**: Bouncing dots animation while AI responds
- **Timestamps**: Each message shows exact time
- **Keyboard Support**: Enter to send, Shift+Enter for new lines

### ?? **Responsive Design**
- Desktop: 360px width, 600px height
- Mobile: Full screen (100% width/height)
- Smooth transitions and animations
- Touch-friendly interface

## Component Structure

### File: `ChatWidget.razor`
Location: `Client.Palaro2026/Components/ChatWidget.razor`

**Key Sections**:
1. **Header** - Fixed at top with Palaro logo, title, and close button
2. **Messages Area** - Scrollable container with custom scrollbar
3. **Input Area** - Auto-expanding textarea with send button
4. **FAB Button** - Fixed floating action button

**Styling Features**:
- Custom CSS variables for easy theming
- Minimalist bubble styles (user vs AI)
- Enhanced message formatting with lists and paragraphs
- Mobile media queries for responsive design

## Message Formatting

The `GetFormattedMessage()` method supports:

### Markdown-like Syntax
```
- Bullet point
* Bullet point
1. Numbered list
# Heading
## Sub-heading
### Sub-sub-heading
**Bold text**
*Italic text*
https://links.com - Clickable links
```

### Output Formatting
- **Lists**: Properly indented with `<ul>` and `<li>` tags
- **Headers**: Converted to bold paragraphs
- **Bold/Italic**: Standard HTML formatting
- **Links**: Clickable URLs with target="_blank"
- **Line Breaks**: Preserved with proper spacing

## Integration Points

### ChatService (`Client.Palaro2026/Services/ChatService.cs`)
```csharp
public interface IChatService
{
    event Action? OnMessagesChanged;
    Task<bool> SendMessageAsync(string userMessage);
    Task<List<ChatMessage>> GetMessagesAsync();
    Task ClearMessagesAsync();
    Task<string> GetResponseAsync(string userMessage);
}
```

### Backend Integration
Currently connects to webhook:
```
https://workflow.pgas.ph/webhook/97125c35-98f6-4ca0-b1d9-665377cadf68/chat
```

To change the endpoint, modify `ChatService.cs`:
```csharp
private readonly string _webhookUrl = "YOUR_NEW_WEBHOOK_URL";
```

## Key Code Methods

### 1. **Auto-Expand Textarea**
```csharp
private async Task AutoExpandTextarea()
{
    await JS.InvokeVoidAsync("eval", "var el = document.getElementById('chatInputBox'); window.expandTextarea(el);");
}
```

### 2. **Send Message**
```csharp
private async Task SendMessage()
{
    // Optimistic UI update
    _messages.Add(new ChatMessage { Content = textToSend, IsUser = true });
    
    // Send to service
    await ChatService.SendMessageAsync(textToSend);
    
    // Refresh messages
    _messages = await ChatService.GetMessagesAsync();
}
```

### 3. **Scroll to Bottom**
```csharp
private async Task ScrollToBottom()
{
    await JS.InvokeVoidAsync("eval", "var el = document.querySelector('.chat-scroll-container'); if(el) el.scrollTop = el.scrollHeight;");
}
```

### 4. **Message Formatting**
```csharp
private string GetFormattedMessage(string content)
{
    // JSON cleaning
    // HTML encoding for security
 // Markdown processing (lists, headers, bold, italic, links)
    // Returns formatted HTML
}
```

## Customization Guide

### Change Colors
Update CSS variables in `<style>` section:
```css
:root {
    --min-primary: #0f172a;   /* Change primary color */
    --min-accent: #f59e0b;       /* Change accent color */
  --min-bg: #ffffff;           /* Change background */
    --min-gray: #f8fafc;         /* Change gray tone */
}
```

### Change Chat Window Size
```css
width: 360px;      /* Chat width */
height: 600px;     /* Chat height */
max-height: 80vh;    /* Mobile height */
```

### Change Position
```css
bottom: 90px;        /* Distance from bottom */
right: 20px;         /* Distance from right */
```

### Change Animation Speed
```css
.chat-window {
    transition: all 0.3s ease-in-out; /* Adjust 0.3s */
}
```

## Data Model

### ChatMessage Class
```csharp
public class ChatMessage
{
    public string Id { get; set; }
    public string Author { get; set; }
    public string Content { get; set; }
    public DateTime Timestamp { get; set; }
    public bool IsUser { get; set; }
}
```

## Features to Add

1. **Message Persistence**: Save to database
2. **Conversation History**: Load previous chats
3. **User Authentication**: Link chats to user accounts
4. **Rating System**: Users can rate AI responses
5. **Rich Media**: Support images/attachments
6. **Voice Messages**: Audio input/output
7. **Analytics**: Track chat interactions
8. **Caching**: Store frequently asked responses

## Browser Compatibility

- ? Chrome/Chromium 90+
- ? Firefox 88+
- ? Safari 14+
- ? Edge 90+
- ?? Internet Explorer: Not supported

## Performance Notes

- Lightweight: ~15KB CSS + JS
- No external dependencies (uses MudBlazor)
- Lazy loads messages (loads on demand)
- Efficient scrolling with virtual scroll potential
- Optimistic UI updates for better UX

## Deployment Checklist

- [ ] Test on mobile devices
- [ ] Verify webhook URL is correct
- [ ] Check CORS settings on backend
- [ ] Test long conversations (100+ messages)
- [ ] Verify scroll performance
- [ ] Test keyboard shortcuts (Enter, Shift+Enter)
- [ ] Check styling on different browsers
- [ ] Test with different message lengths
- [ ] Verify timestamps display correctly
- [ ] Test theme switching (if applicable)

## Support & Debugging

**Enable Debug Mode**:
Add to `Program.cs`:
```csharp
#if DEBUG
    builder.Services.AddLogging(config => config.AddConsole());
#endif
```

**Common Issues**:
1. **Chat button not visible**: Check z-index values
2. **Messages not appearing**: Verify ChatService is registered
3. **Scroll not working**: Check overflow-auto CSS
4. **Formatting broken**: Test message with simpler text first
5. **Performance lag**: Clear browser cache and rebuild

## Team Notes

- Minimalist design follows Palaro brand guidelines
- Code uses Blazor best practices and async/await patterns
- Fully responsive - no additional mobile code needed
- Easily themeable through CSS variables
- Webhook integration allows AI responses via Palaro workflow
