# FocusLens AI Engine

AI engine for the FocusLens adaptive learning platform.

FocusLens AI is the intelligence layer responsible for analyzing educational content and transforming it into structured learning data using Large Language Models (LLMs).

It enables:
- Educational content understanding
- Concept extraction
- AI-generated assessments
- Student answer evaluation
- Adaptive explanations

---

# Overview

FocusLens AI works as a standalone AI service integrated with the main FocusLens backend.

The overall flow:

```
User Content
      |
      v
AI Processing Pipeline
      |
      v
Content Understanding
      |
      v
Learning Structure + Assessments + Explanations
```

Supported input types:

- Text
- PDF documents
- Images

---

# Core AI Capabilities

## 1. Content Understanding

The AI analyzes educational materials and extracts:

- Sections
- Concepts
- Key learning points
- Difficulty levels
- Estimated study duration


Example output:

```json
{
  "status": "success",
  "inputType": "text",
  "content": {
    "sections": [
      {
        "sectionId": "sec_01",
        "title": "Photosynthesis",
        "difficulty": "medium",
        "estimatedTimeMinutes": 20,
        "concepts": [
          {
            "conceptId": "con_01",
            "name": "Energy Conversion",
            "keyLearningPoints": [
              "Plants convert light energy into chemical energy"
            ],
            "difficulty": "medium",
            "estimatedTimeMinutes": 10
          }
        ]
      }
    ]
  }
}
```

---

## 2. AI Question Generation

The AI generates multiple-choice questions based on extracted concepts.

Each generated question includes:

- Question text
- Four answer options
- Correct answer
- Difficulty level
- Explanation
- Section ID
- Concept ID


Flow:

```
Learning Concept
       |
       v
MCQ Generation Service
       |
       v
Assessment Questions
```

---

## 3. Answer Evaluation

The AI evaluates student answers and generates learning signals.

The evaluation returns:

- Whether the answer is correct
- Related section
- Related concept
- Learning state


Supported learning signals:

```
understood
needs_review
```

Flow:

```
Student Answer
       |
       v
AI Evaluation
       |
       v
Learning Signal
```

---

## 4. Adaptive Explanation

The AI generates targeted explanations when a student needs additional support.

The explanation service:

- Focuses on a specific concept
- Uses simple student-friendly language
- Uses only provided educational content
- Preserves the original meaning


Flow:

```
Learning Signal
       |
       v
Explanation Request
       |
       v
AI Generated Explanation
```

---

# Architecture

```
                 FocusLens Backend
                        |
                        |
                        v
                 FocusLens AI Engine
                        |
        ---------------------------------
        |               |               |
        v               v               v
 Content Analysis   MCQ Engine   Explanation Engine
        |
        v
   Gemini AI Model
```

---

# Technology Stack

## Backend

- C#
- .NET 10
- ASP.NET Core Minimal API

## AI

- Gemini API
- Large Language Models (LLMs)
- Structured JSON generation

## Content Processing

- PDF text extraction
- Image content extraction
- Base64 file processing

---

# Project Structure

```
FocusLens.AI

├── Models
│   ├── AIProcessingResult.cs
│   ├── ContentAnalysisResult.cs
│   ├── ContentInputRequest.cs
│   ├── MCQModels.cs
│   ├── AnswerEvaluationModels.cs
│   └── ExplanationModels.cs
│
├── Services
│   ├── GeminiService.cs
│   ├── ContentInputService.cs
│   ├── ContentAnalysisService.cs
│   ├── AIContentPipelineService.cs
│   ├── MCQGenerationService.cs
│   ├── AnswerEvaluationService.cs
│   └── ExplanationService.cs
│
├── Program.cs
└── FocusLens.AI.csproj
```

---

# API Endpoints

## Health Check

```
GET /
```

Returns service status.

---

## AI Content Processing Pipeline

```
POST /ai/process
```

Processes educational content and returns:

- Sections
- Concepts
- Learning points
- Difficulty
- Estimated duration


---

## File Processing

```
POST /ai/process-file
```

Supports:

- PDF files
- Images


---

## Generate MCQs

```
POST /generate-mcq
```

Generates assessment questions from learning concepts.


---

## Evaluate Student Answer

```
POST /evaluate-answer
```

Evaluates answers and returns learning signals.


---

## Generate Explanation

```
POST /ai/explanation
```

Generates concept-level explanations.

---

# Configuration

The service requires a Gemini API key.

For local development:

```bash
dotnet user-secrets set "Gemini:ApiKey" "YOUR_API_KEY"
```

Do not commit API keys or sensitive configuration files.

---

# Running Locally

Clone the repository:

```bash
git clone https://github.com/Focus-Lens/FocusLens.AI.git
```

Navigate to the project:

```bash
cd FocusLens.AI
```

Restore dependencies:

```bash
dotnet restore
```

Run the application:

```bash
dotnet run
```

The API will be available at:

```
http://localhost:5248
```

---

# AI Design Principles

FocusLens AI follows:

- Structured JSON outputs
- Content-grounded generation
- Modular AI services
- Clear backend integration contracts
- Separation between AI processing and application logic

---

# Future Improvements

- Advanced OCR for scanned PDFs
- Enhanced adaptive learning models
- Learning analytics integration
- AI performance monitoring
- Model optimization

---

# FocusLens

Adaptive learning powered by AI.
