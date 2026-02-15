import { useState } from "react"
import { SetupView } from "./components/SetupView.tsx"
import { ChatView } from "./components/ChatView.tsx"
import { TranscriptsView } from "./components/TranscriptsView.tsx"
import type { SetupConfig } from "./types.ts"

export function App() {
  const [view, setView] = useState<"setup" | "chat" | "transcripts">("setup")
  const [config, setConfig] = useState<SetupConfig | null>(null)

  const handleConnect = (cfg: SetupConfig) => {
    setConfig(cfg)
    setView("chat")
  }

  if (view === "setup") {
    return <SetupView onConnect={handleConnect} />
  }

  if (!config) return null

  if (view === "transcripts") {
    return (
      <TranscriptsView
        phoneNumber={config.phoneNumber}
        baseUrl={config.baseUrl}
        onBack={() => setView("chat")}
      />
    )
  }

  return (
    <ChatView
      {...config}
      onViewTranscripts={() => setView("transcripts")}
    />
  )
}
