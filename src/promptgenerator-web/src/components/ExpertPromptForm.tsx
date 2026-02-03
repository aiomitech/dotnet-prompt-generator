"use client";

import { useMemo, useState } from "react";
import { buildApiUrl } from "@/lib/api";

const minProblemLength = 20;
const maxProblemLength = 2000;

type ExpertResponse = {
  success: boolean;
  data?: {
    optimizedPrompt: string;
    details: {
      expertDesign: string;
      methodologyExecution: string;
      optimizedResponse: string;
    };
  };
  error?: string;
};

export default function ExpertPromptForm() {
  const [problem, setProblem] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [result, setResult] = useState<ExpertResponse | null>(null);
  const [activeTab, setActiveTab] = useState<"prompt" | "expert" | "methodology">("prompt");
  const [copied, setCopied] = useState(false);

  const remaining = maxProblemLength - problem.length;
  const isTooShort = problem.trim().length > 0 && problem.trim().length < minProblemLength;

  const canSubmit = useMemo(() => {
    const length = problem.trim().length;
    return length >= minProblemLength && length <= maxProblemLength && !isLoading;
  }, [problem, isLoading]);

  const handleCopy = async (text: string) => {
    try {
      await navigator.clipboard.writeText(text);
      setCopied(true);
      setTimeout(() => setCopied(false), 1500);
    } catch {
      setCopied(false);
    }
  };

  const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setError(null);
    setResult(null);

    const trimmed = problem.trim();
    if (!trimmed) {
      setError("Please describe the problem you want to solve.");
      return;
    }

    if (trimmed.length < minProblemLength) {
      setError(`Please provide at least ${minProblemLength} characters.`);
      return;
    }

    if (trimmed.length > maxProblemLength) {
      setError(`Please keep your request under ${maxProblemLength} characters.`);
      return;
    }

    const controller = new AbortController();
    const timeout = setTimeout(() => controller.abort(), 60000);

    try {
      setIsLoading(true);
      const response = await fetch(buildApiUrl("/api/v1/expert-generate-prompt"), {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({ problem: trimmed }),
        signal: controller.signal,
      });

      const payload = (await response.json()) as ExpertResponse;

      if (!response.ok || !payload.success) {
        setError(payload.error ?? "We couldn't generate an expert prompt just now. Please try again.");
        return;
      }

      setResult(payload);
      setActiveTab("prompt");
    } catch (err) {
      if ((err as Error).name === "AbortError") {
        setError("The request timed out. Please try again.");
      } else {
        setError("Something went wrong. Please try again.");
      }
    } finally {
      clearTimeout(timeout);
      setIsLoading(false);
    }
  };

  return (
    <div className="w-full max-w-2xl mx-auto p-6 bg-white rounded-lg shadow-lg">
      <div className="mb-6">
        <h2 className="text-2xl font-bold text-gray-900 mb-2">Expert Prompt Generator</h2>
        <p className="text-gray-600 text-sm">Advanced three-stage prompt generation with expert persona design</p>
      </div>

      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label htmlFor="problem" className="block text-sm font-medium text-gray-700 mb-2">
            Your Problem or Goal
          </label>
          <textarea
            id="problem"
            value={problem}
            onChange={(e) => setProblem(e.target.value)}
            placeholder="Describe what you need help with... (minimum 20 characters)"
            className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent resize-none"
            rows={5}
            disabled={isLoading}
          />
          <div className="flex justify-between mt-2">
            <span className={`text-sm ${isTooShort ? "text-red-600" : "text-gray-600"}`}>
              {isTooShort ? `At least ${minProblemLength} characters required` : `${remaining} characters remaining`}
            </span>
            <span className="text-sm text-gray-500">{problem.length}</span>
          </div>
        </div>

        {error && (
          <div className="p-4 bg-red-50 border border-red-200 rounded-lg">
            <p className="text-red-800 text-sm">{error}</p>
          </div>
        )}

        <button
          type="submit"
          disabled={!canSubmit}
          className={`w-full py-3 px-4 rounded-lg font-medium transition ${
            canSubmit
              ? "bg-blue-600 text-white hover:bg-blue-700 cursor-pointer"
              : "bg-gray-300 text-gray-500 cursor-not-allowed"
          }`}
        >
          {isLoading ? "Generating Expert Prompt..." : "Generate Expert Prompt"}
        </button>
      </form>

      {result && result.success && result.data && (
        <div className="mt-8 pt-8 border-t border-gray-200">
          <div className="mb-4">
            <div className="flex gap-2">
              <button
                onClick={() => setActiveTab("prompt")}
                className={`px-4 py-2 rounded-lg text-sm font-medium transition ${
                  activeTab === "prompt"
                    ? "bg-blue-600 text-white"
                    : "bg-gray-200 text-gray-700 hover:bg-gray-300"
                }`}
              >
                Optimized Prompt
              </button>
              <button
                onClick={() => setActiveTab("expert")}
                className={`px-4 py-2 rounded-lg text-sm font-medium transition ${
                  activeTab === "expert"
                    ? "bg-blue-600 text-white"
                    : "bg-gray-200 text-gray-700 hover:bg-gray-300"
                }`}
              >
                Expert Design
              </button>
              <button
                onClick={() => setActiveTab("methodology")}
                className={`px-4 py-2 rounded-lg text-sm font-medium transition ${
                  activeTab === "methodology"
                    ? "bg-blue-600 text-white"
                    : "bg-gray-200 text-gray-700 hover:bg-gray-300"
                }`}
              >
                Methodology
              </button>
            </div>
          </div>

          {activeTab === "prompt" && (
            <div>
              <h3 className="text-lg font-semibold text-gray-900 mb-3">Generated Prompt</h3>
              <div className="bg-gray-50 p-4 rounded-lg border border-gray-200 mb-4">
                <p className="text-gray-700 whitespace-pre-wrap text-sm">{result.data.optimizedPrompt}</p>
              </div>
              <button
                onClick={() => handleCopy(result.data.optimizedPrompt)}
                className="bg-green-600 text-white px-4 py-2 rounded-lg text-sm font-medium hover:bg-green-700 transition"
              >
                {copied ? "✓ Copied!" : "Copy Prompt"}
              </button>
            </div>
          )}

          {activeTab === "expert" && (
            <div>
              <h3 className="text-lg font-semibold text-gray-900 mb-3">Expert Design (JSON)</h3>
              <div className="bg-gray-50 p-4 rounded-lg border border-gray-200 max-h-96 overflow-y-auto">
                <pre className="text-gray-700 text-xs whitespace-pre-wrap break-words">
                  {(() => {
                    try {
                      return JSON.stringify(JSON.parse(result.data.details.expertDesign), null, 2);
                    } catch {
                      return result.data.details.expertDesign;
                    }
                  })()}
                </pre>
              </div>
            </div>
          )}

          {activeTab === "methodology" && (
            <div>
              <h3 className="text-lg font-semibold text-gray-900 mb-3">Methodology Execution (JSON)</h3>
              <div className="bg-gray-50 p-4 rounded-lg border border-gray-200 max-h-96 overflow-y-auto">
                <pre className="text-gray-700 text-xs whitespace-pre-wrap break-words">
                  {(() => {
                    try {
                      return JSON.stringify(JSON.parse(result.data.details.methodologyExecution), null, 2);
                    } catch {
                      return result.data.details.methodologyExecution;
                    }
                  })()}
                </pre>
              </div>
            </div>
          )}
        </div>
      )}
    </div>
  );
}
