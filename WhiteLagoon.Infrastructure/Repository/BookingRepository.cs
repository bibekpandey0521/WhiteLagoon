using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using WhiteLagoon.Application.common.interfaces;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Infrastructure.Data;

namespace WhiteLagoon.Infrastructure.Repository
{
    public class BookingRepository : Repository<Booking>, IBookingRepository
    {
        private readonly ApplicationDbContext _db;
        public BookingRepository(ApplicationDbContext db) : base(db) 
        {
            _db = db;

        }
        //public void Add(Villa entity)
        //{
        //    _db.Add(entity);
        //}

        //public Villa Get(Expression<Func<Villa, bool>> filter, string? includeProperties = null)
        //{
        //    //IQueryable<Villa> query = _db.villas;
        //    IQueryable<Villa> query = _db.Set<Villa>();
        //    if (filter != null)
        //    {
        //        query = query.Where(filter);
        //    }
        //    if (!string.IsNullOrEmpty(includeProperties))
        //    {
        //        //Villa,VillaNumber --case sensitive
        //        foreach (var includeProp in includeProperties
        //            .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
        //        {
        //            query = query.Include(includeProp);
        //        }
        //    }
        //    return query.FirstOrDefault();
        //}

        //public IEnumerable<Villa> GetAll(Expression<Func<Villa, bool>>? filter = null, string? includeProperties = null)
        //{
        //    //IQueryable<Villa> query = _db.villas;
        //    IQueryable<Villa> query = _db.Set<Villa>();
        //    if (filter != null)
        //    {
        //        query = query.Where(filter);
        //    }
        //    if (!string.IsNullOrEmpty(includeProperties))
        //    {
        //        //Villa,VillaNumber --case sensitive
        //        foreach (var includeProp in includeProperties
        //            .Split(new char[] {','},StringSplitOptions.RemoveEmptyEntries))
        //        {
        //            query = query.Include(includeProp);
        //        }
        //    }
        //    return query.ToList();
        //}
        //public void Remove(Villa entity)
        //{
        //    _db.Remove(entity);
        //}

        //public void Save()
        //{
        //    _db.SaveChanges();
        //}

        public void Update(Booking entity)
        {
            _db.Bookings.Update(entity);
        }
    }
}
