import { FormEvent, useEffect, useState } from 'react';
import { changeStatus, createTodo, deleteTodo, getTodos } from './api';
import type { NewTodo, Priority, Todo, TodoStatus } from './types';

const empty:NewTodo = { title:'', description:'', priority:'NORMAL', status:'REGISTRERAD' };
const sv = { REGISTRERAD:'Registrerad', STARTAD:'Startad', AVSLUTAD:'Avslutad', LAG:'Låg', NORMAL:'Normal', HOG:'Hög', KRITISK:'Kritisk' } as const;

export default function App(){
  const [todos,setTodos]=useState<Todo[]>([]); const [form,setForm]=useState<NewTodo>(empty); const [error,setError]=useState('');
 const load = () =>
  getTodos()
    .then(setTodos)
    .catch(() =>
      setError('Kunde inte hämta uppgifter. Kontrollera att backend körs.')
    );

useEffect(() => {
  void load();
}, []);
  async function run(action:()=>Promise<unknown>, message:string){
    try { setError(''); await action(); load(); }
    catch { setError(message); }
  }
  async function submit(e:FormEvent){ e.preventDefault(); if(!form.title.trim()) { setError('Ange en rubrik.'); return; } await run(async()=>{ await createTodo(form); setForm(empty); }, 'Kunde inte spara uppgiften.'); }
  async function status(id:number,value:TodoStatus){ await run(()=>changeStatus(id,value), 'Kunde inte ändra status.'); }
  async function remove(id:number){ if(confirm('Vill du ta bort uppgiften?')) await run(()=>deleteTodo(id), 'Kunde inte ta bort uppgiften.'); }
  return <main>
    <header><p className="eyebrow">ASP.NET Core + React</p><h1>Att göra-lista</h1><p>.NET-version med PostgreSQL och komplett statushistorik.</p></header>
    <section className="panel"><h2>Ny uppgift</h2><form onSubmit={submit}>
      <label>Rubrik<input value={form.title} maxLength={200} onChange={e=>setForm({...form,title:e.target.value})}/></label>
      <label>Beskrivning<textarea value={form.description} maxLength={2000} onChange={e=>setForm({...form,description:e.target.value})}/></label>
      <label>Prioritet<select value={form.priority} onChange={e=>setForm({...form,priority:e.target.value as Priority})}><option value="LAG">Låg</option><option value="NORMAL">Normal</option><option value="HOG">Hög</option><option value="KRITISK">Kritisk</option></select></label>
      <button>Lägg till</button>
    </form></section>
    {error && <p className="error">{error}</p>}
    <section className="grid">{todos.map(t=><article className={`card priority-${t.priority}`} key={t.id}>
      <div className="row"><span className={`badge status-${t.status}`}>{sv[t.status]}</span><span className="badge">{sv[t.priority]}</span></div>
      <h2>{t.title}</h2><p>{t.description}</p>
      <label>Status<select value={t.status} onChange={e=>status(t.id,e.target.value as TodoStatus)}><option value="REGISTRERAD">Registrerad</option><option value="STARTAD">Startad</option><option value="AVSLUTAD">Avslutad</option></select></label>
      <details><summary>Historik ({t.history.length})</summary>{t.history.length===0?<p>Inga statusändringar.</p>:<ul>{t.history.map(h=><li key={h.id}>{new Date(h.changedAt).toLocaleString('sv-SE')}: {sv[h.fromStatus]} → {sv[h.toStatus]}</li>)}</ul>}</details>
      <button className="danger" onClick={()=>remove(t.id)}>Ta bort</button>
    </article>)}</section>
  </main>
}
