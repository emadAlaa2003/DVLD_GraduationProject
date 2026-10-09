# DVLD Graduation Project

مشروع تخرج لإدارة معاملات ورخص القيادة، يجمع تطبيق موظفين Windows Forms، وتطبيق مواطن Android، وASP.NET Core API، وبنك أسئلة وشات يعتمدان على وثائق PDF وذكاء اصطناعي محلي.

هذا التعريف محدّث بتاريخ **2026-10-09** وفق الكود المدمج في `main` عند [2dc6cab](https://github.com/emadAlaa2003/DVLD_GraduationProject/commit/2dc6cab). يصف التنفيذ الموجود، ولا يعني أن كل وظائف الموبايل أو صلاحيات الموظفين اكتملت.

## مكونات المشروع

| المجلد | دوره | التقنية |
|---|---|---|
| [DVLD](DVLD/) | إدارة الموظفين والأشخاص والطلبات والرخص والوثائق والأسئلة وحسابات المواطنين | WinForms، .NET Framework 4.8 |
| [DVLD.Mobile](DVLD.Mobile/) | تطبيق المواطن: تسجيل الدخول، Dashboard، وقائمة رخصي | Android، Java، Retrofit، OkHttp، Gson |
| [DVLD.Api](DVLD.Api/) | APIs للمواطن والامتحان والـAI وخدمات المعالجة الخلفية | ASP.NET Core، .NET 10 |
| [DVLD_Buisness](DVLD_Buisness/) | كائنات وقواعد Business | net48;net10.0 |
| [DVLD_DataAccess](DVLD_DataAccess/) | قراءة وكتابة SQL Server باستخدام ADO.NET | net48;net10.0 |
| [DVLD.AI](DVLD.AI/) | PDF، embeddings، الاسترجاع والشات وتوليد الأسئلة | .NET 10 |

الأسماء مكتوبة كما تظهر في المستودع. الموبايل يصل إلى البيانات عبر API → Business → DataAccess → SQL Server؛ تطبيق الموظفين يستخدم Business مباشرة لمعظم الإدارة، وAPI للعمليات المتصلة بالـAI.

## الوظائف الموجودة

- إدارة الأشخاص وحسابات الموظفين والطلبات والمواعيد والرخص المحلية والدولية.
- إدارة حساب المواطن من `frmMobileAccount`: إنشاء الحساب، إعادة كلمة المرور، التفعيل والتعطيل. حسابات `MobileUsers` منفصلة عن `Users` الموظفين، ويُحفظ PasswordHash لحساب المواطن.
- تسجيل دخول المواطن عبر `POST /api/mobile-auth/login` وجلسة Cookie، والتحقق من ملكية قوائم وتفاصيل الرخص والطلبات والمواعيد.
- Android Login ثم Dashboard ببيانات فعلية عن الرخص والطلبات والمواعيد، وشاشة «رخصي» للقوائم المحلية والدولية.
- رفع PDF ومعالجته بالخلفية، وشات RAG يسترجع مقاطع المعرفة قبل بناء الإجابة.
- توليد Multiple Choice وTrue/False من وثيقة جاهزة، مع معلومات المصدر والصفحة والدليل وحفظ Draft للمراجعة البشرية.
- اعتماد الأسئلة أو رفضها، ومتابعة Job التوليد من تطبيق الموظف.
- backend للامتحان الرسمي، مخصص للكمبيوتر في مركز الامتحان وفق نطاق المشروع، وليس امتحانًا رسميًا على الموبايل.

شاشات الموبايل الأخرى لا تُعتبر مكتملة لمجرد وجود عناصر تنقل لها. تفاصيل الرخص المحلية الجاري تطويرها محليًا ليست ضمن نسخة `main` الموثقة هنا.

## دليل شرح الكود

| الفصل | المحتوى |
|---|---|
| [فهرس الدليل](docs/project-guide/README.md) | ترتيب القراءة، البنية وحدود التوثيق |
| [01 — النظام وواجهات الموظفين](docs/project-guide/01-system-and-desktop.md) | الطبقات والأشخاص والطلبات والرخص والوثائق وبنك الأسئلة |
| [02 — معالجة الوثائق والـAI والشات](docs/project-guide/02-ai-documents-and-chat.md) | PDF العربي، التقسيم، embeddings، Qdrant، الاسترجاع وإعادة الترتيب |
| [03 — توليد الأسئلة](docs/project-guide/03-ai-question-generation.md) | اختيار المصادر، التوليد والتحقق والدليل، الحفظ والطابور والـWorker |
| [04 — API والامتحان الرسمي](docs/project-guide/04-api-and-official-exam.md) | تتبع endpoints إلى SQL، بدء الامتحان وحفظ الإجابات والتصحيح |
| [05 — التشغيل والبيانات والمناقشة](docs/project-guide/05-operation-data-and-defense.md) | الخدمات وقاعدة البيانات وسيناريو العرض والقيود |
| [06 — الموبايل وحسابات المواطنين](docs/project-guide/06-mobile-and-citizen-auth.md) | Android، Cookie، إدارة الحساب وحماية الملكية والتشغيل المحلي |

بدأت الفصول 01–05 من مراجعة 6 أكتوبر، وصُححت أوصاف المصادقة والموبايل في هذا التحديث. بعضها يحتوي مراجع `J:/...` تخص جهاز إعداد الدليل؛ استخدم مسارات المستودع النسبية للتصفح على GitHub.

## كيف يعمل الـAI؟

1. يحفظ API وثيقة PDF وبياناتها ويضع مهمة المعالجة في طابور الذاكرة.
2. يستخرج PdfPig النص، وتعالج الأدوات المحلية النص العربي وتقسمه إلى مقاطع مع معلومات المصدر والصفحة.
3. يحسب Ollama embeddings، ويخزن Qdrant المقاطع والمتجهات.
4. للشات: يسترجع المقاطع المرتبطة بالسؤال ويعيد ترتيبها عبر reranker، ثم يرسل السياق إلى نموذج الإجابة.
5. للتوليد: يختار مصادر من الوثيقة، ويطلب أسئلة منظمة، ويتحقق منها ومن دليلها، ثم يحفظها Draft للمراجعة.

هذا مسار RAG، وليس تدريب أوزان أو fine-tuning. وجود المصدر والدليل لا يضمن وحده صحة كل إجابة أو سؤال.

## تشغيل الكمبيوتر والـAPI

1. على Windows افتح [DVLD/DVLD.sln](DVLD/DVLD.sln)، ووفّر .NET 10 SDK وأدوات بناء .NET Framework 4.8، ثم Restore وBuild.
2. استعد قاعدة SQL Server مطابقة للمشروع أو جهّزها من نسخة الفريق. [Database/Migrations](Database/Migrations/) تحديثات جزئية وليست script لإنشاء القاعدة كاملة. تأكد من جدول `MobileUsers` قبل استخدام حسابات المواطنين.
3. اضبط متغير البيئة `DVLD_CONNECTION_STRING` وأعد فتح بيئة التشغيل. لا تضع كلمات مرور الاتصال في Git.
4. شغّل API ثم WinForms بحساب موظف فعّال. التشغيل عبر HTTPS هو المسار المناسب عند تجهيز اتصال آمن.

```powershell
dotnet run --project .\DVLD.Api\DVLD.Api.csproj --launch-profile https
```

عناوين التطوير: `https://localhost:7077` و`http://localhost:5277` وفق [launchSettings.json](DVLD.Api/Properties/launchSettings.json).

## تشغيل Android محليًا

افتح [DVLD.Mobile](DVLD.Mobile/) في Android Studio، ثم Gradle Sync. المشروع يستخدم `compileSdk/targetSdk 36` و`minSdk 26` وJava source compatibility 11؛ استخدم JDK المتوافق مع إصدار Android Gradle Plugin في المشروع.

العميل الحالي في [ApiClient.java](DVLD.Mobile/app/src/main/java/com/dvld/mobile/network/ApiClient.java) يتصل بـ`http://127.0.0.1:5277/`، للتجربة المحلية عبر ADB:

```powershell
dotnet run --project .\DVLD.Api\DVLD.Api.csproj --launch-profile http
adb reverse tcp:5277 tcp:5277
```

شغّل نسخة **Debug** على جهاز/محاكي متصل بـADB. إعداد Debug يسمح باتصال HTTP المحلي؛ Release لا يسمح بهذا الاتصال، فلا يُعتبر هذا الإعداد جاهزًا للنشر. يجب أن يعمل API في `Development` لهذا السيناريو. ملف تشغيل `http` يتجنب التحويل إلى منفذ HTTPS عند التجربة المحلية.

أنشئ حساب المواطن أو أعد كلمة مروره من `People → Manage Mobile Account` في WinForms، ثم استخدم البيانات المعروضة في Android. الجلسة محفوظة بذاكرة عملية التطبيق فقط؛ إغلاق العملية يفقد Cookie. `PersonID` الممرر للشاشات لا يُغني عن Cookie ولا يسمح بقراءة بيانات شخص آخر.

## خدمات الـAI

لا يلزم تشغيل خدمات الـAI لاختبار Login أو قراءة الرخص؛ تحتاجها لمعالجة الوثائق والشات والتوليد:

```powershell
ollama pull qwen3-embedding:0.6b
ollama pull qwen3:1.7b
```

- Ollama للنموذجين المذكورين.
- Qdrant عبر `localhost:6334`، وcollection باسم `dvld_knowledge` بحجم متجه 1024 وCosine.
- reranker محلي على `http://localhost:8081/rerank`؛ تنفيذ خادمه ليس ضمن ملفات المستودع الموثقة.

## حدود التنفيذ والبيانات

- Cookie المواطن مدتها الافتراضية ثماني ساعات دون SlidingExpiration، ويُعاد فحص الحساب وملكية البيانات. في Development تستخدم `SameAsRequest` لدعم التجربة المحلية؛ خارج Development تكون `SecurePolicy.Always`.
- `PeopleController` مغلق أمام الزائر والمواطن بسياسة `EmployeeApiOnly`؛ إعداد مصادقة موظفي API ليس ضمن التنفيذ الحالي، ولا توجد صلاحيات موظفين تفصيلية موثقة كميزة مكتملة.
- الامتحان الرسمي خارج جلسة المواطن، ومخصص لبيئة مركز امتحان مشرف عليها ومعزولة وفق تعليق الكود.
- ملفات `.bak` وبيانات الاتصال ومجلد PDF المرفوع مستثناة من Git. Backup SQL لا يشمل PDF أو Qdrant.
- طوابير الوثائق والتوليد داخل الذاكرة؛ لا يظهر استئناف تلقائي بعد إعادة تشغيل API.
- متابعة التوليد وعرض نتائج الدفعة وحد الثقة للشات وحماية مصادر الأسئلة عند إعادة المعالجة لها قيود موثقة في الدليل.

هذا التحديث توثيق للكود، ولم يتضمن تشغيل Android أو اختبارات تكامل أو Build جديدًا. نجاح Rebuild سابق للمشاريع .NET لا يثبت تشغيل الموبايل أو كل الخدمات الخارجية.
