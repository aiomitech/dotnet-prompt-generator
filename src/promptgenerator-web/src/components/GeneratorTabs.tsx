"use client";

import { useState } from "react";
import PromptForm from "./PromptForm";
import ExpertPromptForm from "./ExpertPromptForm";

export default function GeneratorTabs() {
  const [activeTab, setActiveTab] = useState<"standard" | "expert">("standard");

  return (
    <div className="w-full">
      <div className="flex gap-2 mb-6 border-b border-gray-200">
        <button
          onClick={() => setActiveTab("standard")}
          className={`px-4 py-3 font-medium text-sm transition border-b-2 ${
            activeTab === "standard"
              ? "border-blue-600 text-blue-600"
              : "border-transparent text-gray-600 hover:text-gray-900"
          }`}
        >
          Standard Generator
        </button>
        <button
          onClick={() => setActiveTab("expert")}
          className={`px-4 py-3 font-medium text-sm transition border-b-2 ${
            activeTab === "expert"
              ? "border-blue-600 text-blue-600"
              : "border-transparent text-gray-600 hover:text-gray-900"
          }`}
        >
          Expert Generator
        </button>
      </div>

      {activeTab === "standard" && <PromptForm />}
      {activeTab === "expert" && <ExpertPromptForm />}
    </div>
  );
}
