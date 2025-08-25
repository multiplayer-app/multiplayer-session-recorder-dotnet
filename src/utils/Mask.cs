using System.Text.Json;
using System.Text.Json.Nodes;
using Multiplayer.SessionRecorder.Constants;

public static class Masking
{
    private const int MAX_DEPTH = 8;

    public static readonly HashSet<string> SensitiveFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "password", "pass", "passwd", "pwd", "token", "access_token", "accessToken",
        "refresh_token", "refreshToken", "secret", "api_key", "apiKey", "authorization",
        "auth_token", "authToken", "jwt", "session_id", "sessionId", "sessionToken",
        "client_secret", "clientSecret", "private_key", "privateKey", "public_key",
        "publicKey", "key", "encryption_key", "encryptionKey", "credit_card", "creditCard",
        "card_number", "cardNumber", "cvv", "cvc", "ssn", "sin", "pin", "security_code",
        "securityCode", "bank_account", "bankAccount", "iban", "swift", "bic",
        "routing_number", "routingNumber", "license_key", "licenseKey", "otp", "mfa_code",
        "mfaCode", "phone_number", "phoneNumber", "email", "address", "dob", "tax_id",
        "taxId", "passport_number", "passportNumber", "driver_license", "driverLicense",
        "set-cookie", "cookie", "proxyAuthorization"
    };

    public static readonly HashSet<string> SensitiveHeaders = new(StringComparer.OrdinalIgnoreCase)
    {
        "set-cookie", "cookie", "authorization", "proxyAuthorization"
    };

    public static string MaskJson(string inputJson, IEnumerable<string>? keysToMask = null)
    {
        JsonNode? root;
        try
        {
            root = JsonNode.Parse(inputJson);
        }
        catch
        {
            return inputJson;
        }

        var masked = (keysToMask != null && keysToMask.Any())
            ? MaskSelected(root, new HashSet<string>(keysToMask, StringComparer.OrdinalIgnoreCase))
            : MaskAll(root);

        return masked?.ToJsonString(new JsonSerializerOptions
        {
            WriteIndented = false,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        }) ?? inputJson;
    }

    private static JsonNode? MaskAll(JsonNode? node, int depth = 0)
    {
        if (node == null || depth > MAX_DEPTH)
            return null;

        if (node is JsonArray array)
        {
            for (int i = 0; i < array.Count; i++)
                array[i] = MaskAll(array[i], depth + 1);
        }
        else if (node is JsonObject obj)
        {
            foreach (var key in obj.ToList())
                obj[key.Key] = MaskAll(key.Value, depth + 1);
        }
        else if (node is JsonValue value && value.TryGetValue(out string? strVal))
        {
            return Constants.MASK_PLACEHOLDER;
        }

        return node;
    }

    private static JsonNode? MaskSelected(JsonNode? node, HashSet<string> keysToMask)
    {
        if (node == null)
            return null;

        if (node is JsonArray array)
        {
            for (int i = 0; i < array.Count; i++)
                array[i] = MaskSelected(array[i], keysToMask);
        }
        else if (node is JsonObject obj)
        {
            foreach (var key in obj.ToList())
            {
                if (keysToMask.Contains(key.Key))
                {
                    obj[key.Key] = Constants.MASK_PLACEHOLDER;
                }
                else
                {
                    obj[key.Key] = MaskSelected(key.Value, keysToMask);
                }
            }
        }

        return node;
    }
}
