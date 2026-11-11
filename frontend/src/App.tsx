import { FormEvent, useEffect, useState } from 'react';
import { changeStatus, createTodo, deleteTodo, getTodos } from './api';
import type { NewTodo, Priority, Todo, TodoStatus } from './types';

// Startvärden för formuläret "Ny uppgift". Används även för att tömma formuläret efter sparning.
const empty:NewTodo = { title:'', description:'', priority:'NORMAL', status:'REGISTRERAD' };
// Översätter API:ets enum-värden (t.ex. "HOG") till svensk visningstext (t.ex. "Hög").
const sv = { REGISTRERAD:'Registrerad', STARTAD:'Startad', AVSLUTAD:'Avslutad', LAG:'Låg', NORMAL:'Normal', HOG:'Hög', KRITISK:'Kritisk' } as const;

export default function App(){
  // todos: listan som visas, hämtad från backend.
  // form:  det användaren just nu skriver i formuläret "Ny uppgift".
  // error: felmeddelande som visas ovanför listan (tom sträng = inget fel).
  const [todos,setTodos]=useState<Todo[]>([]); const [form,setForm]=useState<NewTodo>(empty); const [error,setError]=useState('');

  // Hämtar alla uppgifter från backend och ersätter listan i state.
  // Backend är "sanningen": efter varje ändring hämtas hela listan om i stället för
  // att listan uppdateras lokalt. Det gör att t.ex. ny statushistorik alltid syns.
 const load = () =>
  getTodos()
    .then(setTodos)
    .catch(() =>
      setError('Kunde inte hämta uppgifter. Kontrollera att backend körs.')
    );

// Hämta listan en gång när sidan laddas (tom beroendelista = bara vid första renderingen).
useEffect(() => {
  void load();
}, []);

  // Gemensam hjälpfunktion för alla ändringar (skapa, byta status, ta bort):
  // 1. Rensa ett eventuellt tidigare felmeddelande.
  // 2. Kör själva API-anropet (action).
  // 3. Lyckades det: ladda om listan från backend så att ändringen syns.
  // 4. Misslyckades det (nätverksfel, 400, 404, 500 ...): visa det angivna felmeddelandet.
  async function run(action:()=>Promise<unknown>, message:string){
    try { setError(''); await action(); load(); }
    catch { setError(message); }
  }

  // LÄGG TILL: körs när användaren klickar på "Lägg till" (formulärets submit).
  async function submit(e:FormEvent){
    // Stoppa webbläsarens vanliga formulärpost som annars skulle ladda om sidan.
    e.preventDefault();
    // Snabb kontroll i klienten innan anropet; backend validerar också rubriken.
    if(!form.title.trim()) { setError('Ange en rubrik.'); return; }
    // Skicka POST /api/todos. Formuläret töms bara om sparningen lyckades,
    // så att användaren inte tappar det inskrivna vid fel.
    await run(async()=>{ await createTodo(form); setForm(empty); }, 'Kunde inte spara uppgiften.');
  }

  // ÄNDRA STATUS: körs när användaren väljer ett nytt värde i ett kortets status-rullista.
  // Skickar PATCH /api/todos/{id}/status; backend sparar ny status och skapar en historikpost.
  async function status(id:number,value:TodoStatus){ await run(()=>changeStatus(id,value), 'Kunde inte ändra status.'); }

  // TA BORT: körs när användaren klickar på "Ta bort". Användaren måste först bekräfta
  // i en dialog; avbryter de händer ingenting. Annars skickas DELETE /api/todos/{id}.
  async function remove(id:number){ if(confirm('Vill du ta bort uppgiften?')) await run(()=>deleteTodo(id), 'Kunde inte ta bort uppgiften.'); }

  return <main>
    <header><p className="eyebrow">ASP.NET Core + React</p><h1>Att göra-lista</h1><p>.NET-version med PostgreSQL och komplett statushistorik.</p></header>

    {/* Formulär för ny uppgift. Varje fält är "kontrollerat": värdet ligger i state (form)
        och varje tangenttryckning uppdaterar state via setForm. */}
    <section className="panel"><h2>Ny uppgift</h2><form onSubmit={submit}>
      <label>Rubrik<input value={form.title} maxLength={200} onChange={e=>setForm({...form,title:e.target.value})}/></label>
      <label>Beskrivning<textarea value={form.description} maxLength={2000} onChange={e=>setForm({...form,description:e.target.value})}/></label>
      <label>Prioritet<select value={form.priority} onChange={e=>setForm({...form,priority:e.target.value as Priority})}><option value="LAG">Låg</option><option value="NORMAL">Normal</option><option value="HOG">Hög</option><option value="KRITISK">Kritisk</option></select></label>
      <button>Lägg till</button>
    </form></section>

    {/* Felmeddelandet visas bara när error inte är tomt. */}
    {error && <p className="error">{error}</p>}

    {/* Ett kort per uppgift. key={t.id} låter React hålla isär korten när listan ändras. */}
    <section className="grid">{todos.map(t=><article className={`card priority-${t.priority}`} key={t.id}>
      <div className="row"><span className={`badge status-${t.status}`}>{sv[t.status]}</span><span className="badge">{sv[t.priority]}</span></div>
      <h2>{t.title}</h2><p>{t.description}</p>
      {/* Byte av status skickas direkt till backend – det finns ingen separat spara-knapp. */}
      <label>Status<select value={t.status} onChange={e=>status(t.id,e.target.value as TodoStatus)}><option value="REGISTRERAD">Registrerad</option><option value="STARTAD">Startad</option><option value="AVSLUTAD">Avslutad</option></select></label>
      {/* Statushistoriken kommer från backend, redan sorterad med senaste ändringen först. */}
      <details><summary>Historik ({t.history.length})</summary>{t.history.length===0?<p>Inga statusändringar.</p>:<ul>{t.history.map(h=><li key={h.id}>{new Date(h.changedAt).toLocaleString('sv-SE')}: {sv[h.fromStatus]} → {sv[h.toStatus]}</li>)}</ul>}</details>
      <button className="danger" onClick={()=>remove(t.id)}>Ta bort</button>
    </article>)}</section>
  </main>
}
