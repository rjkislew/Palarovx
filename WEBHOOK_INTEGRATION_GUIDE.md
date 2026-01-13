# Chat Widget - Webhook Integration Guide

## ?? Webhook Configuration

Your chat widget is now configured to use the PGAS workflow webhook endpoint:

**Endpoint:** `https://workflow.pgas.ph/webhook/97125c35-98f6-4ca0-b1d9-665377cadf68/chat`

## ?? Request Format

The chat widget sends messages to your webhook in the following JSON format:

```json
{
  "message": "User's message here",
  "sessionId": "unique-session-id-guid"
}
```

**Parameters:**
- `message` (string): The user's chat message
- `sessionId` (string): Unique session ID (automatically generated per chat session)

## ?? Response Format

The webhook should respond with a JSON object. The service will automatically look for the response in any of these formats:

**Option 1: Direct message property**
```json
{
  "message": "Bot response here"
}
```

**Option 2: Response property**
```json
{
  "response": "Bot response here"
}
```

**Option 3: Text property**
```json
{
  "text": "Bot response here"
}
```

**Option 4: Nested in data object**
```json
{
  "data": {
    "message": "Bot response here"
  }
}
```

**Option 5: Raw text response**
```
Bot response as plain text
```

## ?? User Experience Features

### Visual Indicators
- **Typing Animation**: Shows a blinking animation while waiting for bot response
- **Message Timestamps**: Each message displays the time it was sent (HH:mm format)
- **Dark Mode Support**: Automatically adapts to your application's theme
- **Floating Button**: Always accessible in the bottom-right corner

### User Interactions
- **Send Message**: Click the send button or press Enter to submit
- **Open/Close**: Click the floating chat button to expand/collapse
- **Responsive**: Works perfectly on mobile, tablet, and desktop

## ?? Technical Details

### Files Involved
1. **ChatService.cs** - Handles webhook communication
   - Sends requests to your webhook
   - Parses various response formats
   - Manages error handling
   - Maintains session state

2. **ChatWidget.razor** - UI component
   - Displays messages and typing indicator
   - Handles user input
   - Shows loading state during requests

3. **ChatWidget.css** - Styling and animations
   - Responsive design
   - Typing indicator animation
   - Dark mode support

### Session Management
Each chat session gets a unique `sessionId` (GUID) that persists for the browser session. You can use this to track conversations on your backend.

## ?? Features

? **Real-time Communication** - Direct webhook integration
? **Smart Response Parsing** - Handles multiple response formats
? **Error Handling** - Graceful error messages for failed requests
? **Typing Indicator** - Shows when bot is "thinking"
? **Session Tracking** - Each session has a unique ID
? **Dark Mode** - Full theme support
? **Mobile Responsive** - Works on all devices
? **Keyboard Support** - Press Enter to send
? **Accessible** - MudBlazor components

## ?? Error Handling

The service handles these error scenarios:

- **Connection errors**: "Connection error: [error message]"
- **HTTP errors**: "Error from server: [status code]"
- **JSON parsing errors**: Returns raw response content
- **Timeout**: Will show connection timeout message

## ?? Example Backend Implementation

If your workflow needs to send back responses, use one of these formats:

**Node.js/Express Example:**
```javascript
app.post('/webhook/fac799d3-02af-442b-ab59-da60baca7578/chat', (req, res) => {
  const { message, sessionId } = req.body;
  
  // Process the message
  const response = `You said: ${message}`;
  
  res.json({ message: response });
});
```

**Python/Flask Example:**
```python
@app.route('/webhook/fac799d3-02af-442b-ab59-da60baca7578/chat', methods=['POST'])
def chat_webhook():
    data = request.json
    message = data.get('message')
session_id = data.get('sessionId')
    
    response = f"You said: {message}"
    
    return jsonify({'message': response})
```

## ?? Next Steps

1. **Test the Webhook**: Send a test message from the chat widget
2. **Monitor Responses**: Check that your webhook is receiving requests
3. **Customize Responses**: Add bot logic to generate dynamic responses
4. **Track Sessions**: Use sessionId to track conversation history
5. **Add Persistence**: Store messages in a database if needed

## ?? Troubleshooting

| Issue | Solution |
|-------|----------|
| Messages not sending | Check webhook URL is accessible and CORS is configured |
| No response received | Verify webhook returns JSON or text response |
| Typing indicator stuck | Check network tab for failed requests |
| Dark mode not working | Ensure theme classes are applied to parent elements |

## ?? Security Notes

- The webhook URL is hardcoded in the service
- Consider adding authentication headers if needed
- Validate and sanitize all user input on your backend
- Implement rate limiting to prevent abuse
- Use HTTPS for the webhook endpoint

---

**Last Updated**: Now  
**Version**: 1.0  
**Integration**: PGAS Workflow Service
