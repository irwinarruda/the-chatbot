import { useState } from "react"
import { SetupView } from "./components/SetupView.tsx"
import { ChatView } from "./components/ChatView.tsx"
import type { SetupConfig } from "./types.ts"

export function App() {
  const [view, setView] = useState<"setup" | "chat">("setup")
  const [config, setConfig] = useState<SetupConfig | null>(null)

  const handleConnect = (cfg: SetupConfig) => {
    setConfig(cfg)
    setView("chat")
  }

  if (view === "setup") {
    return <SetupView onConnect={handleConnect} />
  }

  if (!config) return null;
  return <ChatView {...config} />
}
