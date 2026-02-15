import { useState, useCallback, useEffect } from "react";
import { useKeyboard } from "@opentui/react";
import { theme } from "../theme.ts";
import type { SetupConfig } from "../types.ts";

const defaultPhone = process.env.TUI_PHONE_NUMBER ?? ""
const defaultUrl = process.env.TUI_BASE_URL ?? "http://localhost:8080"

export function SetupView({
  onConnect,
}: {
  onConnect: (cfg: SetupConfig) => void;
}) {
  const [phone, setPhone] = useState(defaultPhone);
  const [url, setUrl] = useState(defaultUrl);
  const [focused, setFocused] = useState<"phone" | "url">("phone");

  useEffect(() => {
    if (defaultPhone && defaultUrl) {
      onConnect({ phoneNumber: defaultPhone, baseUrl: defaultUrl })
    }
  }, [])

  const handleSubmit = useCallback(() => {
    if (phone) {
      onConnect({ phoneNumber: phone, baseUrl: url });
    }
  }, [phone, url, onConnect]);

  useKeyboard((key) => {
    if (key.name === "tab") {
      setFocused((f) => (f === "phone" ? "url" : "phone"));
    }
  });

  return (
    <box
      flexGrow={1}
      alignItems="center"
      justifyContent="center"
      backgroundColor={theme.neutral[950]}
    >
      <box
        borderStyle="rounded"
        borderColor={theme.green[800]}
        backgroundColor={theme.neutral[900]}
        padding={2}
        flexDirection="column"
        gap={1}
        width={54}
      >
        <box alignItems="center" flexDirection="column" gap={0}>
          <ascii-font font="tiny" text="TheChatbot" />
          <text fg={theme.green[600]}>Terminal Client</text>
        </box>

        <box height={1} />

        <box flexDirection="row" gap={1}>
          <text fg={theme.neutral[400]} width={10}>
            Phone
          </text>
          <box
            border
            borderStyle="single"
            borderColor={
              focused === "phone" ? theme.green[600] : theme.neutral[700]
            }
            width={36}
            height={3}
          >
            <input
              placeholder="5511999999999"
              onInput={setPhone}
              onSubmit={handleSubmit}
              focused={focused === "phone"}
              backgroundColor={theme.neutral[850]}
              focusedBackgroundColor={theme.neutral[800]}
              textColor={theme.neutral[100]}
              cursorColor={theme.green[400]}
            />
          </box>
        </box>

        <box flexDirection="row" gap={1}>
          <text fg={theme.neutral[400]} width={10}>
            URL
          </text>
          <box
            border
            borderStyle="single"
            borderColor={
              focused === "url" ? theme.green[600] : theme.neutral[700]
            }
            width={36}
            height={3}
          >
            <input
              value={url}
              onInput={setUrl}
              onSubmit={handleSubmit}
              focused={focused === "url"}
              backgroundColor={theme.neutral[850]}
              focusedBackgroundColor={theme.neutral[800]}
              textColor={theme.neutral[100]}
              cursorColor={theme.green[400]}
            />
          </box>
        </box>

        <box height={1} />

        <box alignItems="center">
          <text fg={theme.neutral[600]}>
            Tab to switch fields | Enter to connect
          </text>
        </box>
      </box>
    </box>
  );
}
