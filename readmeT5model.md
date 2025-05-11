---
pipeline_tag: text2text-generation
library_name: transformers
tags:
  - t5
  - vietnamese
  - information-extraction
  - text2text-generation
---

# ViT5 Motor Extractor

## Model Card for `letran1110/vit5_motor_extractor`

This is a fine-tuned [ViT5](https://huggingface.co/VietAI/vit5-base) model for extracting motor specifications from raw text descriptions. The model is trained to take in noisy or unstructured motor-related information and output structured key-value pairs such as power, voltage, poles, protection class, and more.

---

## 🧠 Model Details

- **Model Type:** `T5ForConditionalGeneration`
- **Language(s):** Vietnamese (primary), English (partially)
- **Finetuned From:** `VietAI/vit5-base`
- **License:** MIT
- **Framework:** 🤗 Transformers

---

## 🔧 How to Use

```python
from transformers import AutoTokenizer, AutoModelForSeq2SeqLM

tokenizer = AutoTokenizer.from_pretrained("letran1110/vit5_motor_extractor")
model = AutoModelForSeq2SeqLM.from_pretrained("letran1110/vit5_motor_extractor")

text = "Động cơ 3 pha 5.5kW, 4 cực, điện áp 380V, vỏ nhôm, bảo vệ IP55"
inputs = tokenizer(text, return_tensors="pt")
outputs = model.generate(**inputs)
print(tokenizer.decode(outputs[0], skip_special_tokens=True))
```

## ✅ Intended Use
This model is designed to help extract structured information from motor specification descriptions (both Vietnamese and partial English), useful in:

- Inventory parsing

- Industrial cataloging

- Smart search & indexing for motor components

## ❌ Out-of-Scope Use
- Long-form document QA

- General conversation

- Image-based input (OCR must be done separately)

## 📚 Training
Dataset: Custom dataset crawled and annotated from motor product pages

Epochs: 10

Batch Size: 16

Max Length: 512

Optimizer: AdamW

## 🧪 Evaluation
Evaluation is manual by checking structured JSON outputs. Target fields include:
- `motor_name`
- `power`
- `voltage`
- `poles`
- `protection`
- `frame_size`
- `shaft_diameter`
- `material`

## 🤝 Citation
If you use this model, please cite the repo:
```bibtex
@misc{vit5motor2024,
  title={ViT5 Motor Extractor},
  author={letran1110},
  year={2024},
  howpublished={\url{https://huggingface.co/letran1110/vit5_motor_extractor}},
}
```