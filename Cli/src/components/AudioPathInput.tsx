import { useState, useRef, useCallback } from "react"
import type { InputRenderable, KeyEvent } from "@opentui/core"
import { theme } from "../theme.ts"

export function AudioPathInput({
  onSend,
  disabled,
  focused,
}: {
  onSend: (filePath: string) => void
  disabled: boolean
  focused?: boolean
}) {
  const inputRef = useRef<InputRenderable>(null)
  const [suggestions, setSuggestions] = useState<string[]>([])
  const [selectedIndex, setSelectedIndex] = useState(0)
  const debounceRef = useRef<Timer | null>(null)

  const homeDir = Bun.env.HOME || "/tmp"

  const resolvePath = useCallback(
    (path: string) => {
      if (path.startsWith("~/")) return path.replace("~", homeDir)
      if (path.startsWith("~")) return homeDir
      return path
    },
    [homeDir],
  )

  const searchFiles = useCallback(
    async (query: string) => {
      if (!query.trim()) {
        setSuggestions([])
        return
      }
      try {
        var resolved = resolvePath(query)
        var lastSlash = resolved.lastIndexOf("/")
        var dir: string
        var pattern: string
        if (lastSlash >= 0) {
          dir = resolved.substring(0, lastSlash + 1) || "/"
          pattern = resolved.substring(lastSlash + 1)
        } else {
          dir = homeDir
          pattern = resolved
        }
        var args = pattern
          ? ["find", dir, "-maxdepth", "3", "-iname", `*${pattern}*`]
          : ["find", dir, "-maxdepth", "1"]
        var proc = Bun.spawn(args, { stdout: "pipe", stderr: "pipe" })
        var text = await new Response(proc.stdout).text()
        var results = text
          .trim()
          .split("\n")
          .filter(Boolean)
          .map((f) => f.replace(homeDir, "~"))
          .slice(0, 5)
        setSuggestions(results)
        setSelectedIndex(0)
      } catch {
        setSuggestions([])
      }
    },
    [homeDir, resolvePath],
  )

  const handleInput = useCallback(
    (value: string) => {
      if (debounceRef.current) clearTimeout(debounceRef.current)
      debounceRef.current = setTimeout(() => searchFiles(value), 200)
    },
    [searchFiles],
  )

  const handleSubmit = useCallback(
    (value: string) => {
      var filePath = value.trim()
      if (!filePath || disabled) return
      onSend(resolvePath(filePath))
      if (inputRef.current) inputRef.current.value = ""
      setSuggestions([])
    },
    [disabled, onSend, resolvePath],
  )

  const handleKeyDown = useCallback(
    (key: KeyEvent) => {
      if (suggestions.length === 0) return
      if (key.name === "up") {
        key.preventDefault()
        key.stopPropagation()
        setSelectedIndex((i) => Math.max(0, i - 1))
      } else if (key.name === "down") {
        key.preventDefault()
        key.stopPropagation()
        setSelectedIndex((i) => Math.min(suggestions.length - 1, i + 1))
      } else if (key.name === "tab") {
        key.preventDefault()
        key.stopPropagation()
        if (inputRef.current && suggestions[selectedIndex]) {
          inputRef.current.value = suggestions[selectedIndex]
          setSuggestions([])
        }
      } else if (key.name === "return") {
        key.preventDefault()
        key.stopPropagation()
        if (inputRef.current && suggestions[selectedIndex]) {
          inputRef.current.value = suggestions[selectedIndex]
          setSuggestions([])
        }
      } else if (key.name === "escape") {
        key.preventDefault()
        setSuggestions([])
      }
    },
    [suggestions, selectedIndex],
  )

  return (
    <box flexDirection="column" flexShrink={0}>
      {suggestions.length > 0 && (
        <box flexDirection="column" paddingLeft={3}>
          {suggestions.map((file, i) => (
            <text
              key={file}
              fg={
                i === selectedIndex ? theme.accent.cyan : theme.neutral[400]
              }
            >
              {i === selectedIndex ? `▸ ${file}` : `  ${file}`}
            </text>
          ))}
        </box>
      )}
      <box
        paddingLeft={1}
        paddingRight={1}
        height={3}
        flexShrink={0}
        flexDirection="row"
        alignItems="center"
        gap={1}
      >
        <text fg={theme.accent.cyan}>{"@ "}</text>
        <input
          ref={inputRef}
          flexGrow={1}
          placeholder={
            disabled
              ? "Waiting for connection..."
              : "Audio file path (type to search)..."
          }
          onInput={handleInput}
          onSubmit={handleSubmit as unknown as undefined}
          onKeyDown={handleKeyDown as unknown as undefined}
          backgroundColor={theme.neutral[850]}
          focusedBackgroundColor={theme.neutral[800]}
          textColor={theme.neutral[100]}
          cursorColor={theme.accent.cyan}
          focused={focused}
        />
      </box>
    </box>
  )
}
